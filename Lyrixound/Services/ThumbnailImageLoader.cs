using System;
using System.Threading.Tasks;
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
}
