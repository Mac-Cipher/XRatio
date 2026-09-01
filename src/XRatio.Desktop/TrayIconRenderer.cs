using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace XRatio.Desktop;

internal static class TrayIconRenderer
{
    private const int IconSize = 32;
    private const int RecolorThreshold = 52;
    private const string IconAsset = "avares://XRatio/Assets/XRatio-app-icon-v5.png";
    private const string UpdateBadgeColor = "#2F80ED";
    private const string UpdateBadgeOutlineColor = "#08111F";

    public static WindowIcon CreateStopIcon() =>
        CreateStopIcon(updateAvailable: false);

    public static WindowIcon CreateStopIcon(bool updateAvailable) =>
        CreateBaseIconWithColoredCross("#E5484D", updateAvailable);

    // Keep the previous internal name available to callers that still refer
    // to the OFF state while the tray terminology moves to STOP/PAUSE.
    public static WindowIcon CreateOffIcon() => CreateStopIcon();

    public static WindowIcon CreatePauseIcon() =>
        CreatePauseIcon(updateAvailable: false);

    public static WindowIcon CreatePauseIcon(bool updateAvailable) =>
        CreateBaseIconWithColoredCross("#F59E0B", updateAvailable);

    public static WindowIcon CreateMonochromeIcon() =>
        CreateMonochromeIcon(updateAvailable: false);

    public static WindowIcon CreateMonochromeIcon(bool updateAvailable) =>
        CreateBaseIconWithColoredCross("#F4F7FB", updateAvailable);

    public static WindowIcon CreateColorIcon(bool updateAvailable = false) =>
        CreateBaseIconWithColoredCross("#FFFFFF", updateAvailable);

    private static WindowIcon CreateBaseIconWithColoredCross(string colorHex, bool updateAvailable)
        => new(RenderIcon(colorHex, updateAvailable));

    internal static WriteableBitmap RenderIcon(string colorHex, bool updateAvailable)
    {
        using var source = new Bitmap(AssetLoader.Open(new Uri(IconAsset)));
        using var scaled = source.CreateScaledBitmap(
            new PixelSize(IconSize, IconSize),
            BitmapInterpolationMode.HighQuality);
        var output = new WriteableBitmap(
            new PixelSize(IconSize, IconSize),
            new Vector(96, 96),
            PixelFormat.Rgba8888,
            AlphaFormat.Unpremul);

        using (var framebuffer = output.Lock())
        {
            // Copying through the locked framebuffer asks Avalonia to convert
            // the PNG's native format to the known RGBA layout before the X
            // is recolored. This keeps the original rounded square and its
            // antialiasing exactly as authored in the app icon.
            scaled.CopyPixels(framebuffer);
            RecolorBrightPixels(framebuffer, Color.Parse(colorHex));
            if (updateAvailable)
                DrawUpdateBadge(framebuffer);
        }

        return output;
    }

    private static void DrawUpdateBadge(ILockedFramebuffer framebuffer)
    {
        const float centerX = 25.5f;
        const float centerY = 25.5f;
        const float outerRadius = 5.6f;
        const float innerRadius = 4.25f;

        var outline = Color.Parse(UpdateBadgeOutlineColor);
        var blue = Color.Parse(UpdateBadgeColor);
        for (var y = 19; y < framebuffer.Size.Height; y++)
        {
            for (var x = 19; x < framebuffer.Size.Width; x++)
            {
                var distance = MathF.Sqrt(
                    MathF.Pow(x + 0.5f - centerX, 2) +
                    MathF.Pow(y + 0.5f - centerY, 2));
                if (distance <= outerRadius)
                    WritePixel(framebuffer, x, y, outline);
                if (distance <= innerRadius)
                    WritePixel(framebuffer, x, y, blue);
            }
        }

        // A two-pixel white download arrow keeps the badge readable at the
        // small sizes used by the Windows notification area.
        var arrow = Color.Parse("#FFFFFF");
        for (var y = 21; y <= 24; y++)
        {
            WritePixel(framebuffer, 25, y, arrow);
            WritePixel(framebuffer, 26, y, arrow);
        }
        for (var x = 23; x <= 28; x++)
            WritePixel(framebuffer, x, 25, arrow);
        for (var x = 24; x <= 27; x++)
            WritePixel(framebuffer, x, 26, arrow);
        WritePixel(framebuffer, 25, 27, arrow);
        WritePixel(framebuffer, 26, 27, arrow);
    }

    private static void WritePixel(ILockedFramebuffer framebuffer, int x, int y, Color color)
    {
        if (x < 0 || x >= framebuffer.Size.Width || y < 0 || y >= framebuffer.Size.Height)
            return;

        var pixel = IntPtr.Add(framebuffer.Address, y * framebuffer.RowBytes + x * 4);
        Marshal.WriteByte(pixel, color.R);
        Marshal.WriteByte(IntPtr.Add(pixel, 1), color.G);
        Marshal.WriteByte(IntPtr.Add(pixel, 2), color.B);
        Marshal.WriteByte(IntPtr.Add(pixel, 3), color.A);
    }

    private static void RecolorBrightPixels(ILockedFramebuffer framebuffer, Color color)
    {
        var address = framebuffer.Address;
        for (var y = 0; y < framebuffer.Size.Height; y++)
        {
            for (var x = 0; x < framebuffer.Size.Width; x++)
            {
                var pixel = IntPtr.Add(address, y * framebuffer.RowBytes + x * 4);
                var red = Marshal.ReadByte(pixel);
                var green = Marshal.ReadByte(IntPtr.Add(pixel, 1));
                var blue = Marshal.ReadByte(IntPtr.Add(pixel, 2));
                var luminance = (red * 299 + green * 587 + blue * 114) / 1000;
                if (luminance <= RecolorThreshold)
                    continue;

                // The X is white in the source icon. Use its brightness as
                // coverage so its soft shadow and antialiased edges remain
                // intact when the glyph changes from white to a status color.
                var coverage = Math.Clamp(
                    (luminance - RecolorThreshold) / (255f - RecolorThreshold),
                    0f,
                    1f);
                Marshal.WriteByte(pixel, (byte)Math.Round(color.R * coverage));
                Marshal.WriteByte(IntPtr.Add(pixel, 1), (byte)Math.Round(color.G * coverage));
                Marshal.WriteByte(IntPtr.Add(pixel, 2), (byte)Math.Round(color.B * coverage));
            }
        }
    }
}
