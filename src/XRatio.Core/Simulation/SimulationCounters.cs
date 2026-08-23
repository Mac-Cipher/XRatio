namespace XRatio.Core.Simulation;

public sealed class SimulationCounters
{
    public SimulationCounters(long totalSize, double initialCompletedPercent)
    {
        if (totalSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalSize));
        if (initialCompletedPercent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(initialCompletedPercent));
        TotalSize = totalSize;
        Downloaded = (long)Math.Floor(totalSize * (initialCompletedPercent / 100d));
        Left = totalSize - Downloaded;
    }

    public long TotalSize { get; }
    public long Uploaded { get; private set; }
    public long Downloaded { get; private set; }
    public long Left { get; private set; }

    public bool Advance(TimeSpan elapsed, long uploadBytesPerSecond, long downloadBytesPerSecond)
    {
        if (elapsed < TimeSpan.Zero || uploadBytesPerSecond < 0 || downloadBytesPerSecond < 0)
            throw new ArgumentOutOfRangeException(nameof(elapsed));

        Uploaded = checked(Uploaded + (long)Math.Floor(uploadBytesPerSecond * elapsed.TotalSeconds));
        var wasIncomplete = Left > 0;
        if (wasIncomplete)
        {
            var delta = (long)Math.Floor(downloadBytesPerSecond * elapsed.TotalSeconds);
            Downloaded = Math.Min(TotalSize, checked(Downloaded + delta));
            Left = TotalSize - Downloaded;
        }
        return wasIncomplete && Left == 0;
    }
}

