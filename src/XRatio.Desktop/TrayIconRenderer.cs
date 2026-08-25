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

    public static WindowIcon CreateStopIcon() =>
        CreateBaseIconWithColoredCross("#E5484D");

    // Keep the previous internal name available to callers that still refer
    // to the OFF state while the tray terminology moves to STOP/PAUSE.
    public static WindowIcon CreateOffIcon() => CreateStopIcon();

    public static WindowIcon CreatePauseIcon() =>
        CreateBaseIconWithColoredCross("#F59E0B");

    public static WindowIcon CreateMonochromeIcon() =>
        CreateBaseIconWithColoredCross("#F4F7FB");

    private static WindowIcon CreateBaseIconWithColoredCross(string colorHex)
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
        }

        return new WindowIcon(output);
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
