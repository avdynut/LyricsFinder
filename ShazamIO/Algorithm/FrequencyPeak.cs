namespace ShazamIO.Algorithm;

/// <summary>
/// Represents a frequency peak detected during audio fingerprinting.
/// </summary>
public class FrequencyPeak
{
    public int FftPassNumber { get; set; }
    public int PeakMagnitude { get; set; }
    public int CorrectedPeakFrequencyBin { get; set; }
    public int SampleRateHz { get; set; }

    public FrequencyPeak(int fftPassNumber, int peakMagnitude, int correctedPeakFrequencyBin, int sampleRateHz)
    {
        FftPassNumber = fftPassNumber;
        PeakMagnitude = peakMagnitude;
        CorrectedPeakFrequencyBin = correctedPeakFrequencyBin;
        SampleRateHz = sampleRateHz;
    }

    /// <summary>
    /// Gets the frequency in Hz from the bin position.
    /// Convert back FFT bin to a frequency, given a 16 KHz sample rate, 1024 useful bins 
    /// and the multiplication by 64 made before storing the information.
    /// </summary>
    public double GetFrequencyHz()
    {
        return CorrectedPeakFrequencyBin * (SampleRateHz / 2.0 / 1024.0 / 64.0);
    }

    /// <summary>
    /// Gets the amplitude in PCM.
    /// </summary>
    public double GetAmplitudePcm()
    {
        return Math.Sqrt(Math.Exp((PeakMagnitude - 6144) / 1477.3) * (1 << 17) / 2.0) / 1024.0;
    }

    /// <summary>
    /// Gets the time in seconds.
    /// Assumes that new FFT bins are emitted every 128 samples, on a standard 16 KHz sample rate basis.
    /// </summary>
    public double GetSeconds()
    {
        return (FftPassNumber * 128.0) / SampleRateHz;
    }
}
