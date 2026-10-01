using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Storage.Streams;

namespace Lyrixound.Services;

internal static class ThumbnailImageLoader
{
    public static async Task<byte[]> LoadBytesAsync(object thumbnail)
    {
        if (thumbnail is not IRandomAccessStreamReference reference)
            return null;

        using var stream = await reference.OpenReadAsync();
        if (stream.Size is 0 or > int.MaxValue)
            return null;

        var size = (uint)stream.Size;
        var reader = new DataReader(stream.GetInputStreamAt(0));
        try
        {
            await reader.LoadAsync(size);
            var bytes = new byte[size];
            reader.ReadBytes(bytes);
            return bytes;
        }
        finally
        {
            reader.DetachStream();
            reader.Dispose();
        }
    }

    public static bool LooksLikeAppIcon(byte[] bytes)
    {
        try
        {
            using var stream = new MemoryStream(bytes);
            var frame = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0];
            if (Math.Max(frame.PixelWidth, frame.PixelHeight) <= 64)
                return true;

            var converted = new FormatConvertedBitmap(frame, PixelFormats.Bgra32, null, 0);
            var width = converted.PixelWidth;
            var height = converted.PixelHeight;
            var pixels = new byte[width * height * 4];
            converted.CopyPixels(pixels, width * 4, 0);

            var transparent = 0;
            var sampled = 0;
            var step = Math.Max(1, width * height / 4096);
            for (var i = 0; i < width * height; i += step)
            {
                sampled++;
                if (pixels[i * 4 + 3] < 200)
                    transparent++;
            }

            return sampled > 0 && (double)transparent / sampled > 0.1;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
