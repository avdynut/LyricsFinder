using ShazamIO.Enums;

namespace ShazamIO.Algorithm;

/// <summary>
/// Generates Shazam audio signatures from PCM audio samples.
/// </summary>
public class SignatureGenerator
{
    private static readonly double[] HanningMatrix;

    static SignatureGenerator()
    {
        // Create Hanning window (wipe trailing and leading zeroes)
        HanningMatrix = new double[2048];
        for (int i = 0; i < 2048; i++)
        {
            HanningMatrix[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * (i + 1) / 2050));
        }
    }

    private readonly List<short> _inputPendingProcessing = new();
    private int _samplesProcessed;
    private RingBuffer<short> _ringBufferOfSamples;
    private RingBuffer<double[]> _fftOutputs;
    private RingBuffer<double[]> _spreadFftOutput;

    public double MaxTimeSeconds { get; set; } = 3.1;
    public int MaxPeaks { get; set; } = 255;

    public DecodedMessage NextSignature { get; private set; }

    public int SamplesProcessed
    {
        get => _samplesProcessed;
        set => _samplesProcessed = value;
    }

    public SignatureGenerator()
    {
        _ringBufferOfSamples = new RingBuffer<short>(2048, 0);
        _fftOutputs = new RingBuffer<double[]>(256, new double[1025]);
        _spreadFftOutput = new RingBuffer<double[]>(256, new double[1025]);

        NextSignature = new DecodedMessage
        {
            SampleRateHz = 16000,
            NumberSamples = 0,
            FrequencyBandToSoundPeaks = new Dictionary<FrequencyBand, List<FrequencyPeak>>()
        };
    }

    /// <summary>
    /// Feeds input samples to be processed for signature generation.
    /// Expects signed 16-bit 16 KHz mono PCM samples.
    /// </summary>
    public void FeedInput(short[] samples)
    {
        _inputPendingProcessing.AddRange(samples);
    }

    /// <summary>
    /// Feeds input samples from bytes (little-endian 16-bit samples).
    /// </summary>
    public void FeedInput(byte[] bytes)
    {
        var samples = new short[bytes.Length / 2];
        Buffer.BlockCopy(bytes, 0, samples, 0, bytes.Length);
        FeedInput(samples);
    }

    /// <summary>
    /// Consumes some of the samples fed to FeedInput and returns a Shazam signature.
    /// Returns null if there are no more samples to be consumed.
    /// </summary>
    public DecodedMessage? GetNextSignature()
    {
        if (_inputPendingProcessing.Count - _samplesProcessed < 128)
            return null;

        while (_inputPendingProcessing.Count - _samplesProcessed >= 128 &&
               (NextSignature.NumberSamples / (double)NextSignature.SampleRateHz < MaxTimeSeconds ||
                GetTotalPeakCount() < MaxPeaks))
        {
            var samples = _inputPendingProcessing.GetRange(_samplesProcessed, 128).ToArray();
            ProcessInput(samples);
            _samplesProcessed += 128;
        }

        // Reset buffers for next signature
        _ringBufferOfSamples = new RingBuffer<short>(2048, 0);
        _fftOutputs = new RingBuffer<double[]>(256, new double[1025]);
        _spreadFftOutput = new RingBuffer<double[]>(256, new double[1025]);

        return NextSignature;
    }

    private int GetTotalPeakCount()
    {
        return NextSignature.FrequencyBandToSoundPeaks.Values.Sum(peaks => peaks.Count);
    }

    private void ProcessInput(short[] samples)
    {
        NextSignature.NumberSamples += samples.Length;

        for (int position = 0; position < samples.Length; position += 128)
        {
            var batch = new short[Math.Min(128, samples.Length - position)];
            Array.Copy(samples, position, batch, 0, batch.Length);
            DoFft(batch);
            DoPeakSpreadingAndRecognition();
        }
    }

    private void DoFft(short[] batchOf128Samples)
    {
        // Add samples to ring buffer
        for (int i = 0; i < batchOf128Samples.Length; i++)
        {
            _ringBufferOfSamples[_ringBufferOfSamples.Position + i] = batchOf128Samples[i];
        }
        _ringBufferOfSamples.Position += batchOf128Samples.Length;

        // Get excerpt from ring buffer
        var excerpt = new double[2048];
        for (int i = 0; i < 2048; i++)
        {
            excerpt[i] = _ringBufferOfSamples[_ringBufferOfSamples.Position + i] * HanningMatrix[i];
        }

        // Perform FFT
        var fftResults = PerformRealFft(excerpt);

        // Calculate power spectrum
        var powerSpectrum = new double[1025];
        for (int i = 0; i < 1025; i++)
        {
            powerSpectrum[i] = (fftResults[i].Real * fftResults[i].Real + 
                               fftResults[i].Imaginary * fftResults[i].Imaginary) / (1 << 17);
            powerSpectrum[i] = Math.Max(powerSpectrum[i], 0.0000000001);
        }

        _fftOutputs.Append(powerSpectrum);
    }

    private static System.Numerics.Complex[] PerformRealFft(double[] input)
    {
        int n = input.Length;
        var complex = new System.Numerics.Complex[n];
        for (int i = 0; i < n; i++)
        {
            complex[i] = new System.Numerics.Complex(input[i], 0);
        }

        // Cooley-Tukey FFT
        Fft(complex);

        // Return only first half + 1 (real FFT result)
        var result = new System.Numerics.Complex[n / 2 + 1];
        Array.Copy(complex, result, n / 2 + 1);
        return result;
    }

    private static void Fft(System.Numerics.Complex[] buffer)
    {
        int n = buffer.Length;
        if (n <= 1) return;

        // Bit-reversal permutation
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1)
            {
                j ^= bit;
            }
            j ^= bit;

            if (i < j)
            {
                (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
            }
        }

        // Cooley-Tukey iterative FFT
        for (int len = 2; len <= n; len <<= 1)
        {
            double angle = -2 * Math.PI / len;
            var wlen = new System.Numerics.Complex(Math.Cos(angle), Math.Sin(angle));
            
            for (int i = 0; i < n; i += len)
            {
                var w = System.Numerics.Complex.One;
                for (int j = 0; j < len / 2; j++)
                {
                    var u = buffer[i + j];
                    var t = w * buffer[i + j + len / 2];
                    buffer[i + j] = u + t;
                    buffer[i + j + len / 2] = u - t;
                    w *= wlen;
                }
            }
        }
    }

    private void DoPeakSpreadingAndRecognition()
    {
        DoPeakSpreading();
        if (_spreadFftOutput.NumWritten >= 46)
        {
            DoPeakRecognition();
        }
    }

    private void DoPeakSpreading()
    {
        var originLastFft = _fftOutputs[_fftOutputs.Position - 1];
        var spreadFft = new double[1025];

        // Spread across frequency bins
        for (int i = 0; i < 1025; i++)
        {
            double maxVal = originLastFft[i];
            if (i + 1 < 1025) maxVal = Math.Max(maxVal, originLastFft[i + 1]);
            if (i + 2 < 1025) maxVal = Math.Max(maxVal, originLastFft[i + 2]);
            spreadFft[i] = maxVal;
        }

        // Spread across time (with previous FFT results)
        int i1 = (_spreadFftOutput.Position - 1 + _spreadFftOutput.BufferSize) % _spreadFftOutput.BufferSize;
        int i2 = (_spreadFftOutput.Position - 3 + _spreadFftOutput.BufferSize) % _spreadFftOutput.BufferSize;
        int i3 = (_spreadFftOutput.Position - 6 + _spreadFftOutput.BufferSize) % _spreadFftOutput.BufferSize;

        var prev1 = _spreadFftOutput[i1];
        var prev2 = _spreadFftOutput[i2];
        var prev3 = _spreadFftOutput[i3];

        for (int i = 0; i < 1025; i++)
        {
            prev1[i] = Math.Max(spreadFft[i], prev1[i]);
            prev2[i] = Math.Max(prev1[i], prev2[i]);
            prev3[i] = Math.Max(prev2[i], prev3[i]);
        }

        _spreadFftOutput[i1] = prev1;
        _spreadFftOutput[i2] = prev2;
        _spreadFftOutput[i3] = prev3;
        _spreadFftOutput.Append((double[])spreadFft.Clone());
    }

    private void DoPeakRecognition()
    {
        var fftMinus46 = _fftOutputs[(_fftOutputs.Position - 46 + _fftOutputs.BufferSize) % _fftOutputs.BufferSize];
        var fftMinus49 = _spreadFftOutput[(_spreadFftOutput.Position - 49 + _spreadFftOutput.BufferSize) % _spreadFftOutput.BufferSize];

        for (int binPosition = 10; binPosition < 1015; binPosition++)
        {
            // Ensure that the bin is large enough to be a peak
            if (!(fftMinus46[binPosition] >= 1.0 / 64.0) || 
                !(fftMinus46[binPosition] >= fftMinus49[binPosition - 1]))
                continue;

            // Ensure that it is frequency-domain local minimum
            double maxNeighborInFftMinus49 = 0;
            foreach (var neighborOffset in new[] { -10, -7, -4, -3, 1, 2, 5, 8 })
            {
                var idx = binPosition + neighborOffset;
                if (idx >= 0 && idx < 1025)
                    maxNeighborInFftMinus49 = Math.Max(fftMinus49[idx], maxNeighborInFftMinus49);
            }

            if (!(fftMinus46[binPosition] > maxNeighborInFftMinus49))
                continue;

            // Ensure that it is a time-domain local minimum
            double maxNeighborInOtherAdjacentFfts = maxNeighborInFftMinus49;
            foreach (var otherOffset in new[] { -53, -45, 165, 172, 179, 186, 193, 200, 214, 221, 228, 235, 242, 249 })
            {
                var spreadIdx = (_spreadFftOutput.Position + otherOffset + _spreadFftOutput.BufferSize) % _spreadFftOutput.BufferSize;
                var binIdx = binPosition - 1;
                if (binIdx >= 0 && binIdx < 1025)
                    maxNeighborInOtherAdjacentFfts = Math.Max(_spreadFftOutput[spreadIdx][binIdx], maxNeighborInOtherAdjacentFfts);
            }

            if (!(fftMinus46[binPosition] > maxNeighborInOtherAdjacentFfts))
                continue;

            // This is a peak, store it
            var fftNumber = _spreadFftOutput.NumWritten - 46;

            var peakMagnitude = Math.Log(Math.Max(1.0 / 64.0, fftMinus46[binPosition])) * 1477.3 + 6144;
            var peakMagnitudeBefore = Math.Log(Math.Max(1.0 / 64.0, fftMinus46[binPosition - 1])) * 1477.3 + 6144;
            var peakMagnitudeAfter = Math.Log(Math.Max(1.0 / 64.0, fftMinus46[binPosition + 1])) * 1477.3 + 6144;

            var peakVariation1 = peakMagnitude * 2 - peakMagnitudeBefore - peakMagnitudeAfter;
            var peakVariation2 = (peakMagnitudeAfter - peakMagnitudeBefore) * 32 / peakVariation1;

            var correctedPeakFrequencyBin = binPosition * 64 + peakVariation2;

            if (peakVariation1 <= 0)
                continue;

            var frequencyHz = correctedPeakFrequencyBin * (16000.0 / 2.0 / 1024.0 / 64.0);

            FrequencyBand band;
            if (frequencyHz > 250 && frequencyHz < 520)
                band = FrequencyBand.Hz250_520;
            else if (frequencyHz >= 520 && frequencyHz < 1450)
                band = FrequencyBand.Hz520_1450;
            else if (frequencyHz >= 1450 && frequencyHz < 3500)
                band = FrequencyBand.Hz1450_3500;
            else if (frequencyHz >= 3500 && frequencyHz <= 5500)
                band = FrequencyBand.Hz3500_5500;
            else
                continue;

            if (!NextSignature.FrequencyBandToSoundPeaks.ContainsKey(band))
                NextSignature.FrequencyBandToSoundPeaks[band] = new List<FrequencyPeak>();

            NextSignature.FrequencyBandToSoundPeaks[band].Add(
                new FrequencyPeak(fftNumber, (int)peakMagnitude, (int)correctedPeakFrequencyBin, 16000));
        }
    }
}
