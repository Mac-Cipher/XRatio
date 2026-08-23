using System.Runtime.Versioning;
using XRatio.Desktop.Platform;

namespace XRatio.Desktop.Tests;

[SupportedOSPlatform("linux")]
public sealed class LinuxAutostartServiceTests
{
    [Fact]
    public async Task EnableAndDisable_ManageOnlyTheInjectedDesktopEntry()
    {
        var root = CreateTempDirectory();
        var path = Path.Combine(root, "autostart", "XRatio.desktop");
        var service = new LinuxAutostartService(
            path,
            () => "\"/opt/Ratio Ghost/XRatio\" --minimized");
        try
        {
            Assert.True(service.Capability.IsSupported);
            Assert.False(await service.IsEnabledAsync());

            await service.SetEnabledAsync(true);

            Assert.True(await service.IsEnabledAsync());
            var content = await File.ReadAllTextAsync(path);
            Assert.Contains("Type=Application", content);
            Assert.Contains("Exec=\"/opt/Ratio Ghost/XRatio\" --minimized", content);
            Assert.Contains("X-XRatio-Managed=true", content);

            await service.SetEnabledAsync(false);

            Assert.False(File.Exists(path));
            Assert.False(await service.IsEnabledAsync());
        }
        finally
        {
            DeleteTempDirectory(root);
        }
    }

    [Fact]
    public async Task UnmanagedCollision_IsNeverOverwrittenOrRemoved()
    {
        var root = CreateTempDirectory();
        var path = Path.Combine(root, "autostart", "XRatio.desktop");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        const string unmanaged = "[Desktop Entry]\nType=Application\nName=Other app\n";
        await File.WriteAllTextAsync(path, unmanaged);
        var service = new LinuxAutostartService(
            path,
            () => "\"/opt/XRatio\" --minimized");
        try
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SetEnabledAsync(true));
            Assert.Equal(unmanaged, await File.ReadAllTextAsync(path));
            Assert.False(await service.IsEnabledAsync());

            await service.SetEnabledAsync(false);

            Assert.True(File.Exists(path));
            Assert.Equal(unmanaged, await File.ReadAllTextAsync(path));
        }
        finally
        {
            DeleteTempDirectory(root);
        }
    }

    [Fact]
    public async Task UnmanagedCollisionCreatedDuringEnable_IsNeverOverwritten()
    {
        var root = CreateTempDirectory();
        var path = Path.Combine(root, "autostart", "XRatio.desktop");
        const string unmanaged = "[Desktop Entry]\nType=Application\nName=Other app\n";
        var service = new LinuxAutostartService(
            path,
            () =>
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, unmanaged);
                return "\"/opt/XRatio\" --minimized";
            });
        try
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SetEnabledAsync(true));
            Assert.Equal(unmanaged, await File.ReadAllTextAsync(path));
            Assert.Empty(Directory.GetFiles(root, "*.tmp", SearchOption.AllDirectories));
        }
        finally
        {
            DeleteTempDirectory(root);
        }
    }

    [Fact]
    public async Task CanceledDisable_PreservesManagedEntry()
    {
        var root = CreateTempDirectory();
        var path = Path.Combine(root, "autostart", "XRatio.desktop");
        var service = new LinuxAutostartService(
            path,
            () => "\"/opt/XRatio\" --minimized");
        using var cancellation = new CancellationTokenSource();
        try
        {
            await service.SetEnabledAsync(true);
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => service.SetEnabledAsync(false, cancellation.Token));
            Assert.True(File.Exists(path));
            Assert.True(await service.IsEnabledAsync());
        }
        finally
        {
            DeleteTempDirectory(root);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "XRatio.LinuxAutostartTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteTempDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }
}

