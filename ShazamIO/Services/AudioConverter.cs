using NAudio.Wave;
using ShazamIO.Algorithm;

namespace ShazamIO.Services;

/// <summary>
/// Service for converting and processing audio files.
/// </summary>
public class AudioConverter
{
    private const int TargetSampleRate = 16000;
    private const int TargetBitsPerSample = 16;
    private const int TargetChannels = 1;

    /// <summary>
    /// Normalizes audio data to 16-bit, 16 KHz, mono PCM.
    /// </summary>
    public static byte[] NormalizeAudioData(byte[] audioData)
    {
        using var inputStream = new MemoryStream(audioData);
        using var reader = new StreamMediaFoundationReader(inputStream);
        
        var outFormat = new WaveFormat(TargetSampleRate, TargetBitsPerSample, TargetChannels);
        using var resampler = new MediaFoundationResampler(reader, outFormat);
        resampler.ResamplerQuality = 60;

        using var outputStream = new MemoryStream();
        var buffer = new byte[resampler.WaveFormat.AverageBytesPerSecond];
        int bytesRead;
        while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
        {
            outputStream.Write(buffer, 0, bytesRead);
        }

        return outputStream.ToArray();
    }

    /// <summary>
    /// Reads an audio file and returns normalized PCM data.
    /// </summary>
    public static async Task<byte[]> ReadAudioFileAsync(string filePath)
    {
        var audioData = await File.ReadAllBytesAsync(filePath);
        return NormalizeAudioData(audioData);
    }

    /// <summary>
    /// Normalizes audio bytes from any supported format.
    /// </summary>
    public static byte[] NormalizeAudioBytes(byte[] audioBytes)
    {
        return NormalizeAudioData(audioBytes);
    }

    /// <summary>
    /// Creates a signature generator from normalized audio data.
    /// </summary>
    public static SignatureGenerator CreateSignatureGenerator(byte[] normalizedAudio)
    {
        var generator = new SignatureGenerator();
        generator.FeedInput(normalizedAudio);
        generator.MaxTimeSeconds = 12;

        // If audio is longer than 36 seconds, skip to the middle
        var durationSeconds = normalizedAudio.Length / 2.0 / TargetSampleRate;
        if (durationSeconds > 12 * 3)
        {
            generator.SamplesProcessed += TargetSampleRate * ((int)(durationSeconds / 2) - 6);
        }

        return generator;
    }

    /// <summary>
    /// Creates the search data payload for Shazam API.
    /// </summary>
    public static object CreateSearchData(string timezone, string uri, int sampleMs, long timestamp)
    {
        return new
        {
            timezone,
            signature = new { uri, samplems = sampleMs },
            timestamp,
            context = new { },
            geolocation = new { }
        };
    }
}
