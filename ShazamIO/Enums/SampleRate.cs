namespace ShazamIO.Enums;

/// <summary>
/// Sample rate values used in Shazam signatures.
/// </summary>
public enum SampleRate
{
    Hz8000 = 1,
    Hz11025 = 2,
    Hz16000 = 3,
    Hz32000 = 4,
    Hz44100 = 5,
    Hz48000 = 6
}

public static class SampleRateExtensions
{
    public static int ToHz(this SampleRate sampleRate) => sampleRate switch
    {
        SampleRate.Hz8000 => 8000,
        SampleRate.Hz11025 => 11025,
        SampleRate.Hz16000 => 16000,
        SampleRate.Hz32000 => 32000,
        SampleRate.Hz44100 => 44100,
        SampleRate.Hz48000 => 48000,
        _ => throw new ArgumentOutOfRangeException(nameof(sampleRate))
    };

    public static SampleRate FromHz(int hz) => hz switch
    {
        8000 => SampleRate.Hz8000,
        11025 => SampleRate.Hz11025,
        16000 => SampleRate.Hz16000,
        32000 => SampleRate.Hz32000,
        44100 => SampleRate.Hz44100,
        48000 => SampleRate.Hz48000,
        _ => throw new ArgumentException($"Unsupported sample rate: {hz}", nameof(hz))
    };
}
