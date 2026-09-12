using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using XRatio.Core.Announcements;

namespace XRatio.Desktop;

public sealed partial class MainWindow
{
    private void ApplyEditorNames()
    {
        (Control Editor, string Label)[] fields =
        [
            (_torrentPath, "Torrent file"), (_simulationAccountName, "Account"),
            (_simulationTracker, "Tracker"), (_simulationInfoHash, "Info hash"),
            (_simulationInfoSize, "Size"), (_simulationAnnounceInterval, "Update interval (s)"),
            (_simulationCompleted, "Finished (%)"), (_simulationStopValue, "Stop"),
            (_port, "HTTP proxy port"), (_minimumPeers, "Minimum leechers"),
            (_themeMode, "Theme"), (_accentColor, "Accent color"), (_languageMode, "Language"),
            (_trayIconStyle, "Tray icon"), (_simulationClient, "Client"), (_simulationStopMode, "Stop"),
            (_simulationUploadRate, "Upload speed (kB/s)"), (_simulationDownloadRate, "Download speed (kB/s)")
        ];
        foreach (var (editor, label) in fields)
            AutomationProperties.SetName(editor, L(label));
        AutomationProperties.SetName(_simulationRandomUploadMin, $"{L("Upload speed (kB/s)")} — {L("Min")}");
        AutomationProperties.SetName(_simulationRandomUploadMax, $"{L("Upload speed (kB/s)")} — {L("Max")}");
        AutomationProperties.SetName(_simulationRandomDownloadMin, $"{L("Download speed (kB/s)")} — {L("Min")}");
        AutomationProperties.SetName(_simulationRandomDownloadMax, $"{L("Download speed (kB/s)")} — {L("Max")}");
    }

    private static string OnboardingCompletionLabel(OnboardingStep step) =>
        step.Id is OnboardingStepIds.Interception or OnboardingStepIds.Simulation
            ? "Guide reviewed" : "Completed";

    private void ConfigureGuideAccessibility(Border guide, TextBlock title, Button close, Button done)
    {
        AutomationProperties.SetName(close, L("Close"));
        AutomationProperties.SetName(guide, title.Text);
        close.Click += (_, _) => ReturnToOnboardingStep();
        guide.KeyDown += (_, e) =>
        {
            if (e.Key != Key.Escape) return;
            guide.IsVisible = false;
            ReturnToOnboardingStep();
            e.Handled = true;
        };
    }

    private void ReturnToOnboardingStep()
    {
        _tabs.SelectedIndex = 0;
        var button = _onboardingSidebarRows.FirstOrDefault(row => row.StepIndex == _onboardingStepIndex)?.Button;
        button?.BringIntoView();
        button?.Focus();
    }

    // Reflow pairs when their content no longer fits. The other Overview rows
    // move together, preserving the failure banner, trust note and onboarding.
    private static void MakeAdaptivePair(Grid grid, Control first, Control second,
        double threshold, int row, double firstWeight, double secondWeight)
    {
        bool? stacked = null;
        grid.SizeChanged += (_, _) =>
        {
            var narrow = grid.Bounds.Width < threshold;
            if (stacked == narrow) return;
            var previous = stacked;
            stacked = narrow;
            if (row == 1)
            {
                foreach (var child in grid.Children.Where(c => c != first && c != second))
                {
                    var childRow = Grid.GetRow(child);
                    if (narrow && childRow > row) Grid.SetRow(child, childRow + 1);
                    else if (!narrow && previous == true && childRow > row + 1) Grid.SetRow(child, childRow - 1);
                    Grid.SetColumnSpan(child, narrow ? 1 : 2);
                }
            }
            grid.ColumnDefinitions = narrow
                ? new ColumnDefinitions("*")
                : new ColumnDefinitions(FormattableString.Invariant($"{firstWeight}*,{secondWeight}*"));
            grid.RowDefinitions = new RowDefinitions(row == 1
                ? narrow ? "Auto,Auto,Auto,Auto,Auto" : "Auto,Auto,Auto,Auto"
                : narrow ? "Auto,Auto" : "Auto");
            Grid.SetColumn(first, 0);
            Grid.SetRow(first, row);
            Grid.SetColumn(second, narrow ? 0 : 1);
            Grid.SetRow(second, narrow ? row + 1 : row);
        };
    }

    private Button CreateScreenshotExpandButton()
    {
        var button = CreateButton("Enlarge screenshot", ButtonTone.Secondary, 120);
        button.Tag = "EnlargeOnboardingScreenshot";
        button.HorizontalAlignment = HorizontalAlignment.Left;
        button.Click += async (_, _) =>
        {
            using var stream = AssetLoader.Open(new Uri("avares://XRatio/Assets/qbittorrent-proxy-settings.png"));
            using var bitmap = new Bitmap(stream);
            var close = CreateButton(L("Close"), ButtonTone.Secondary, 90);
            var picture = new Image
            {
                Source = new CroppedBitmap(bitmap, new PixelRect(176, 328, 809, 319)),
                Width = 809, Height = 319, Stretch = Stretch.Uniform
            };
            AutomationProperties.SetName(picture, L("Connect your torrent client"));
            var dialog = new Window
            {
                Title = L("Connect your torrent client"), Width = 880, Height = 490,
                MinWidth = 480, MinHeight = 320, Background = XRatioPalette.Canvas,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new Grid
                {
                    Margin = new Thickness(18), RowDefinitions = new RowDefinitions("*,Auto"), RowSpacing = 14,
                    Children =
                    {
                        new ScrollViewer
                        {
                            Content = picture,
                            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
                        },
                        Place(close, row: 1)
                    }
                }
            };
            close.Click += (_, _) => dialog.Close();
            dialog.KeyDown += (_, e) => { if (e.Key == Key.Escape) { dialog.Close(); e.Handled = true; } };
            dialog.Opened += (_, _) => close.Focus();
            await dialog.ShowDialog(this);
            button.Focus();
        };
        return button;
    }

    private Grid BuildTransferComparison(TorrentSnapshot snapshot)
    {
        var grid = new Grid
        {
            Tag = "TransferComparison", ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 20, Margin = new Thickness(0, 5, 0, 4)
        };
        Control Column(string label, long down, long up, long left) => new StackPanel
        {
            Spacing = 2,
            Children =
            {
                new TextBlock { Text = L(label), FontSize = 12, FontWeight = FontWeight.SemiBold, Foreground = XRatioPalette.Ink },
                new TextBlock
                {
                    Text = $"↓ {FormatBytes(down)}   ↑ {FormatBytes(up)}   {L("Remaining")}: {FormatBytes(left)}",
                    FontSize = 12, Foreground = XRatioPalette.Muted,
                    FontFeatures = XRatioPalette.TabularNumbers, TextWrapping = TextWrapping.Wrap
                }
            }
        };
        grid.Children.Add(Column("Actual", snapshot.ActualDownloaded, snapshot.ActualUploaded, snapshot.ActualLeft));
        grid.Children.Add(Place(Column("Reported", snapshot.ReportedDownloaded, snapshot.ReportedUploaded, snapshot.ReportedLeft), column: 1));
        return grid;
    }
}
