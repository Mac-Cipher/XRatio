using XRatio.Core.Announcements;
using XRatio.Core.Configuration;

namespace XRatio.Core.Tests;

public sealed class AnnounceTransformerTests
{
    [Fact]
    public void Transform_BlocksNonTrackerTrafficByDefault()
    {
        var transformer = new AnnounceTransformer(new FixedRandomSource());

        var result = transformer.Transform(new Uri("http://example.test/index.html"), new XRatioSettings());

        Assert.Equal(AnnounceDisposition.BlockedNonTracker, result.Disposition);
        Assert.Null(result.Target);
    }

    [Fact]
    public void Transform_PortsFreeLeechAndPretendSeedForCompletedTorrent()
    {
        var transformer = new AnnounceTransformer(new FixedRandomSource());
        var settings = new XRatioSettings
        {
            ReportDownloadAsZero = true,
            PretendToSeed = true
        };

        transformer.Transform(
            new Uri("http://tracker.test/announce?info_hash=abc&downloaded=50&uploaded=20&left=700&event=started"),
            settings);
        var result = transformer.Transform(
            new Uri("http://tracker.test/announce?info_hash=abc&downloaded=50&uploaded=20&left=0&event=completed"),
            settings);

        Assert.Equal(AnnounceDisposition.Rewritten, result.Disposition);
        Assert.Contains("downloaded=0", result.Target!.Query, StringComparison.Ordinal);
        Assert.Contains("uploaded=20", result.Target.Query, StringComparison.Ordinal);
        Assert.Contains("left=0", result.Target.Query, StringComparison.Ordinal);
        Assert.DoesNotContain("event=", result.Target.Query, StringComparison.Ordinal);
    }

    [Fact]
    public void Transform_PretendSeedDoesNotHideRemainingBytesForActiveDownload()
    {
        var transformer = new AnnounceTransformer(new FixedRandomSource());
        var settings = new XRatioSettings
        {
            ReportDownloadAsZero = true,
            PretendToSeed = true
        };

        var result = transformer.Transform(
            new Uri("http://tracker.test/announce?info_hash=active&downloaded=50&uploaded=20&left=700"),
            settings);

        Assert.Equal(AnnounceDisposition.Rewritten, result.Disposition);
        Assert.Contains("downloaded=0", result.Target!.Query, StringComparison.Ordinal);
        Assert.Contains("uploaded=20", result.Target.Query, StringComparison.Ordinal);
        Assert.Contains("left=700", result.Target.Query, StringComparison.Ordinal);
    }

    [Fact]
    public void Transform_NormalizesBinaryUrlInfoHashForTorrentNameLookup()
    {
        var bytes = Enumerable.Range(0, 20).Select(value => (byte)value).ToArray();
        var encoded = string.Concat(bytes.Select(value => $"%{value:X2}"));
        var expected = Convert.ToHexString(bytes);
        var transformer = new AnnounceTransformer(new FixedRandomSource());

        var result = transformer.Transform(
            new Uri($"http://tracker.test/announce?info_hash={encoded}&downloaded=1&uploaded=2&left=3"),
            new XRatioSettings());

        Assert.Equal(expected, result.InfoHash);
        Assert.Equal(expected, Assert.Single(transformer.GetSnapshots()).InfoHash);
    }

    [Fact]
    public void Transform_UsesPeerThresholdAndLegacyRatios()
    {
        var transformer = new AnnounceTransformer(new FixedRandomSource(0.5));
        var settings = new XRatioSettings
        {
            MinimumPeers = 5,
            UploadPerDownloadMinimum = 2,
            UploadPerDownloadMaximum = 2,
            UploadPerUploadMinimum = 3,
            UploadPerUploadMaximum = 3,
            BoostChancePercent = 0
        };
        transformer.ObserveTrackerResponse("abc", new TrackerResponse(null, 5, null, null));

        var result = transformer.Transform(
            new Uri("http://tracker.test/announce?info_hash=abc&downloaded=10&uploaded=4&left=1"),
            settings);

        Assert.Contains("uploaded=36", result.Target!.Query, StringComparison.Ordinal);
    }

    [Fact]
    public void Standby_FollowsKnownTorrentLeftValues()
    {
        var transformer = new AnnounceTransformer(new FixedRandomSource());
        var settings = new XRatioSettings();

        transformer.Transform(
            new Uri("http://tracker.test/announce?info_hash=a&downloaded=10&uploaded=4&left=0"),
            settings);
        Assert.True(transformer.IsSeedOnlyStandby);
        transformer.Transform(
            new Uri("http://tracker.test/announce?info_hash=b&downloaded=10&uploaded=4&left=42"),
            settings);
        Assert.True(transformer.HasActiveDownloads);
        Assert.False(transformer.IsSeedOnlyStandby);
    }

    [Fact]
    public void Transform_WhenPausedPreservesPreviouslyReportedCounters()
    {
        var transformer = new AnnounceTransformer(new FixedRandomSource());
        var settings = new XRatioSettings
        {
            ReportDownloadAsZero = false,
            PretendToSeed = false
        };
        transformer.Transform(
            new Uri("http://tracker.test/announce?info_hash=a&downloaded=100&uploaded=200&left=10"),
            settings);

        var paused = transformer.Transform(
            new Uri("http://tracker.test/announce?info_hash=a&downloaded=5&uploaded=7&left=10"),
            settings,
            paused: true);

        Assert.Contains("downloaded=100", paused.Target!.Query, StringComparison.Ordinal);
        Assert.Contains("uploaded=200", paused.Target.Query, StringComparison.Ordinal);
    }

    [Fact]
    public void Transform_SaturatesReportedUploadAtCounterLimit()
    {
        var transformer = new AnnounceTransformer(new FixedRandomSource());
        transformer.Restore(new[]
        {
            new PersistedTorrentState(
                "a",
                "tracker.test",
                ActualFirstLeft: 0,
                ActualDownloaded: 0,
                ActualUploaded: 0,
                ActualLeft: 1,
                ReportedDownloaded: 0,
                ReportedUploaded: long.MaxValue,
                ReportedLeft: 1,
                CompletePeers: 0,
                IncompletePeers: 0,
                LastAnnounce: DateTimeOffset.UnixEpoch)
        });

        var result = transformer.Transform(
            new Uri("http://tracker.test/announce?info_hash=a&downloaded=1&uploaded=1&left=1&event=started"),
            new XRatioSettings());

        Assert.Equal(AnnounceDisposition.Rewritten, result.Disposition);
        Assert.Contains($"uploaded={long.MaxValue}", result.Target!.Query, StringComparison.Ordinal);
    }

    [Fact]
    public void Restore_PreservesReportedCountersAcrossRestartAndCounterReset()
    {
        var settings = new XRatioSettings
        {
            MinimumPeers = 5,
            UploadPerDownloadMinimum = 0,
            UploadPerDownloadMaximum = 0,
            UploadPerUploadMinimum = 4,
            UploadPerUploadMaximum = 4,
            BoostChancePercent = 0
        };
        var beforeRestart = new AnnounceTransformer(new FixedRandomSource());
        beforeRestart.Transform(
            new Uri("http://tracker.test/announce?info_hash=a&downloaded=50&uploaded=100&left=20&event=started"),
            settings,
            now: DateTimeOffset.Parse("2026-08-06T10:00:00Z"));
        beforeRestart.ObserveTrackerResponse("a", new TrackerResponse(null, 5, null, null));
        beforeRestart.Transform(
            new Uri("http://tracker.test/announce?info_hash=a&downloaded=60&uploaded=110&left=20"),
            settings,
            now: DateTimeOffset.Parse("2026-08-06T10:10:00Z"));

        var persisted = beforeRestart.GetPersistedStates();
        var afterRestart = new AnnounceTransformer(new FixedRandomSource());
        afterRestart.Restore(persisted);

        var resumed = afterRestart.Transform(
            new Uri("http://tracker.test/announce?info_hash=a&downloaded=60&uploaded=5&left=20&event=started"),
            settings,
            now: DateTimeOffset.Parse("2026-08-06T11:00:00Z"));

        Assert.Contains("uploaded=150", resumed.Target!.Query, StringComparison.Ordinal);
        Assert.DoesNotContain("uploaded=5", resumed.Target.Query, StringComparison.Ordinal);
    }

    [Fact]
    public void Snapshots_AccumulateActualAndReportedDeltasLikeTcl()
    {
        var transformer = new AnnounceTransformer(new FixedRandomSource());
        var settings = new XRatioSettings
        {
            ReportDownloadAsZero = false,
            PretendToSeed = false
        };
        transformer.Transform(
            new Uri("http://tracker.test/announce?info_hash=a&downloaded=100&uploaded=200&left=50&event=started"),
            settings);
        transformer.Transform(
            new Uri("http://tracker.test/announce?info_hash=a&downloaded=150&uploaded=260&left=0"),
            settings);

        var snapshot = Assert.Single(transformer.GetSnapshots());
        Assert.Equal("tracker.test", snapshot.Tracker);
        Assert.Equal(50, snapshot.ActualDownloadedTotal);
        Assert.Equal(60, snapshot.ActualUploadedTotal);
        Assert.Equal(50, snapshot.ReportedDownloadedTotal);
        Assert.Equal(60, snapshot.ReportedUploadedTotal);
        Assert.Equal(0, snapshot.ActualLeft);
    }

    [Fact]
    public void ResetTorrent_RemovesOnlySelectedTorrentState()
    {
        var transformer = new AnnounceTransformer(new FixedRandomSource());
        var settings = new XRatioSettings();
        transformer.Transform(
            new Uri("http://tracker.test/announce?info_hash=a&downloaded=10&uploaded=20&left=0"),
            settings);
        transformer.Transform(
            new Uri("http://tracker.test/announce?info_hash=b&downloaded=30&uploaded=40&left=50"),
            settings);
        transformer.ObserveTrackerResponse("a", new TrackerResponse(7, 3, 60, null));

        Assert.True(transformer.ResetTorrent("a"));
        var remaining = Assert.Single(transformer.GetSnapshots());
        Assert.Equal("b", remaining.InfoHash);
        Assert.False(transformer.ResetTorrent("a"));
        Assert.True(transformer.HasActiveDownloads);
    }

    [Fact]
    public void Snapshots_IncludeSeedsAndLeechersFromTrackerResponse()
    {
        var transformer = new AnnounceTransformer(new FixedRandomSource());
        transformer.Transform(
            new Uri("http://tracker.test/announce?info_hash=a&downloaded=1&uploaded=2&left=3"),
            new XRatioSettings());

        transformer.ObserveTrackerResponse("a", new TrackerResponse(11, 7, 60, null));

        var snapshot = Assert.Single(transformer.GetSnapshots());
        Assert.Equal(11, snapshot.CompletePeers);
        Assert.Equal(7, snapshot.IncompletePeers);
    }

    [Fact]
    public void Transform_EvictsOldStateAtThePersistenceQuota()
    {
        var transformer = new AnnounceTransformer(new FixedRandomSource());
        var settings = new XRatioSettings();
        for (var index = 0; index <= XRatioSettings.MaxPersistedTorrents; index++)
        {
            transformer.Transform(
                new Uri($"http://tracker.test/announce?info_hash={index}&downloaded=1&uploaded=2&left=3"),
                settings,
                now: DateTimeOffset.UnixEpoch.AddSeconds(index));
        }

        var snapshots = transformer.GetSnapshots();
        Assert.Equal(XRatioSettings.MaxPersistedTorrents, snapshots.Count);
        Assert.DoesNotContain(snapshots, item => item.InfoHash == "0");
        Assert.Contains(snapshots, item => item.InfoHash == XRatioSettings.MaxPersistedTorrents.ToString());
    }

    [Fact]
    public void Transform_RejectsTrackerUriUserInfo()
    {
        var result = new AnnounceTransformer().Transform(
            new Uri("http://user:secret@tracker.test/announce?info_hash=a&downloaded=1&uploaded=2&left=3"),
            new XRatioSettings());

        Assert.Equal(AnnounceDisposition.RejectedInvalid, result.Disposition);
        Assert.Null(result.Target);
    }

    [Fact]
    public void Settings_RejectRemoteUnauthenticatedProxyListening()
    {
        var settings = new XRatioSettings { OnlyLocalConnections = false };

        Assert.Throws<InvalidOperationException>(() => settings.Validate());
    }

    private sealed class FixedRandomSource(double value = 0) : IRandomSource
    {
        public double NextDouble() => value;
    }
}

