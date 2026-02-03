using System.Runtime.InteropServices;
using ShazamIO.Enums;

namespace ShazamIO.Algorithm;

/// <summary>
/// Raw signature header structure (little-endian).
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RawSignatureHeader
{
    public uint Magic1;           // Fixed 0xCAFE2580
    public uint Crc32;            // CRC-32 for all following data
    public uint SizeMinusHeader;  // Total size minus header (48 bytes)
    public uint Magic2;           // Fixed 0x94119C00
    public uint Void1_1;
    public uint Void1_2;
    public uint Void1_3;
    public uint ShiftedSampleRateId;  // SampleRate left-shifted by 27
    public uint Void2_1;
    public uint Void2_2;
    public uint NumberSamplesPlusDividedSampleRate;
    public uint FixedValue;       // ((15 << 19) + 0x40000) = 0x7C0000
}

/// <summary>
/// Represents a decoded Shazam audio signature message.
/// </summary>
public class DecodedMessage
{
    private const string DataUriPrefix = "data:audio/vnd.shazam.sig;base64,";
    private const uint Magic1Value = 0xCAFE2580;
    private const uint Magic2Value = 0x94119C00;

    public int SampleRateHz { get; set; }
    public int NumberSamples { get; set; }
    public Dictionary<FrequencyBand, List<FrequencyPeak>> FrequencyBandToSoundPeaks { get; set; } = new();

    /// <summary>
    /// Decodes a DecodedMessage from binary data.
    /// </summary>
    public static DecodedMessage DecodeFromBinary(byte[] data)
    {
        var message = new DecodedMessage();
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        // Read and check the header
        var header = ReadHeader(reader);

        if (header.Magic1 != Magic1Value)
            throw new InvalidDataException($"Invalid magic1: expected 0x{Magic1Value:X8}, got 0x{header.Magic1:X8}");
        if (header.Magic2 != Magic2Value)
            throw new InvalidDataException($"Invalid magic2: expected 0x{Magic2Value:X8}, got 0x{header.Magic2:X8}");
        if (header.SizeMinusHeader != data.Length - 48)
            throw new InvalidDataException($"Invalid size: expected {data.Length - 48}, got {header.SizeMinusHeader}");

        // Verify CRC32
        var crcData = new byte[data.Length - 8];
        Array.Copy(data, 8, crcData, 0, crcData.Length);
        var calculatedCrc = Crc32Helper.Compute(crcData);
        if (calculatedCrc != header.Crc32)
            throw new InvalidDataException($"CRC32 mismatch: expected 0x{header.Crc32:X8}, got 0x{calculatedCrc:X8}");

        // Extract sample rate
        var sampleRateId = (int)(header.ShiftedSampleRateId >> 27);
        message.SampleRateHz = ((SampleRate)sampleRateId).ToHz();
        message.NumberSamples = (int)(header.NumberSamplesPlusDividedSampleRate - message.SampleRateHz * 0.24);

        // Read the first chunk (fixed, no value)
        var chunkType = reader.ReadUInt32();
        var chunkSize = reader.ReadUInt32();

        if (chunkType != 0x40000000)
            throw new InvalidDataException($"Invalid first chunk type: expected 0x40000000, got 0x{chunkType:X8}");

        // Read frequency peaks
        while (stream.Position < stream.Length)
        {
            if (stream.Position + 8 > stream.Length) break;

            var frequencyBandId = reader.ReadUInt32();
            var frequencyPeaksSize = reader.ReadUInt32();

            if (frequencyPeaksSize == 0) continue;

            var frequencyBand = (FrequencyBand)(frequencyBandId - 0x60030040);
            var peaksData = reader.ReadBytes((int)frequencyPeaksSize);
            var padding = (4 - (frequencyPeaksSize % 4)) % 4;
            if (padding > 0 && stream.Position + padding <= stream.Length)
                reader.ReadBytes((int)padding);

            message.FrequencyBandToSoundPeaks[frequencyBand] = DecodePeaks(peaksData, message.SampleRateHz);
        }

        return message;
    }

    private static RawSignatureHeader ReadHeader(BinaryReader reader)
    {
        return new RawSignatureHeader
        {
            Magic1 = reader.ReadUInt32(),
            Crc32 = reader.ReadUInt32(),
            SizeMinusHeader = reader.ReadUInt32(),
            Magic2 = reader.ReadUInt32(),
            Void1_1 = reader.ReadUInt32(),
            Void1_2 = reader.ReadUInt32(),
            Void1_3 = reader.ReadUInt32(),
            ShiftedSampleRateId = reader.ReadUInt32(),
            Void2_1 = reader.ReadUInt32(),
            Void2_2 = reader.ReadUInt32(),
            NumberSamplesPlusDividedSampleRate = reader.ReadUInt32(),
            FixedValue = reader.ReadUInt32()
        };
    }

    private static List<FrequencyPeak> DecodePeaks(byte[] data, int sampleRateHz)
    {
        var peaks = new List<FrequencyPeak>();
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        var fftPassNumber = 0;

        while (stream.Position < stream.Length)
        {
            var rawFftPass = reader.ReadByte();

            if (rawFftPass == 0xFF)
            {
                if (stream.Position + 4 > stream.Length) break;
                fftPassNumber = (int)reader.ReadUInt32();
                continue;
            }

            fftPassNumber += rawFftPass;

            if (stream.Position + 4 > stream.Length) break;

            var peakMagnitude = reader.ReadUInt16();
            var correctedPeakFrequencyBin = reader.ReadUInt16();

            peaks.Add(new FrequencyPeak(fftPassNumber, peakMagnitude, correctedPeakFrequencyBin, sampleRateHz));
        }

        return peaks;
    }

    /// <summary>
    /// Encodes the message to binary data.
    /// </summary>
    public byte[] EncodeToBinary()
    {
        using var contentStream = new MemoryStream();
        using var contentWriter = new BinaryWriter(contentStream);

        foreach (var kvp in FrequencyBandToSoundPeaks.OrderBy(k => (int)k.Key))
        {
            var frequencyBand = kvp.Key;
            var peaks = kvp.Value;

            using var peaksStream = new MemoryStream();
            using var peaksWriter = new BinaryWriter(peaksStream);

            var fftPassNumber = 0;

            foreach (var peak in peaks)
            {
                if (peak.FftPassNumber - fftPassNumber >= 255)
                {
                    peaksWriter.Write((byte)0xFF);
                    peaksWriter.Write((uint)peak.FftPassNumber);
                    fftPassNumber = peak.FftPassNumber;
                }

                peaksWriter.Write((byte)(peak.FftPassNumber - fftPassNumber));
                peaksWriter.Write((ushort)peak.PeakMagnitude);
                peaksWriter.Write((ushort)peak.CorrectedPeakFrequencyBin);

                fftPassNumber = peak.FftPassNumber;
            }

            var peaksData = peaksStream.ToArray();
            contentWriter.Write((uint)(0x60030040 + (int)frequencyBand));
            contentWriter.Write((uint)peaksData.Length);
            contentWriter.Write(peaksData);

            var padding = (4 - (peaksData.Length % 4)) % 4;
            for (var i = 0; i < padding; i++)
                contentWriter.Write((byte)0);
        }

        var contentsData = contentStream.ToArray();

        // Build header
        var header = new RawSignatureHeader
        {
            Magic1 = Magic1Value,
            Magic2 = Magic2Value,
            ShiftedSampleRateId = (uint)SampleRateExtensions.FromHz(SampleRateHz) << 27,
            FixedValue = (15 << 19) + 0x40000,
            NumberSamplesPlusDividedSampleRate = (uint)(NumberSamples + SampleRateHz * 0.24),
            SizeMinusHeader = (uint)(contentsData.Length + 8)
        };

        // Build full message
        using var outputStream = new MemoryStream();
        using var outputWriter = new BinaryWriter(outputStream);

        // Write header placeholder (will rewrite with CRC later)
        WriteHeader(outputWriter, header);

        // Write first chunk
        outputWriter.Write(0x40000000u);
        outputWriter.Write((uint)(contentsData.Length + 8));

        // Write contents
        outputWriter.Write(contentsData);

        // Calculate CRC32 and rewrite header
        var outputData = outputStream.ToArray();
        var crcData = new byte[outputData.Length - 8];
        Array.Copy(outputData, 8, crcData, 0, crcData.Length);
        header.Crc32 = Crc32Helper.Compute(crcData);

        // Rewrite header with CRC
        outputStream.Position = 0;
        WriteHeader(outputWriter, header);

        return outputStream.ToArray();
    }

    private static void WriteHeader(BinaryWriter writer, RawSignatureHeader header)
    {
        writer.Write(header.Magic1);
        writer.Write(header.Crc32);
        writer.Write(header.SizeMinusHeader);
        writer.Write(header.Magic2);
        writer.Write(header.Void1_1);
        writer.Write(header.Void1_2);
        writer.Write(header.Void1_3);
        writer.Write(header.ShiftedSampleRateId);
        writer.Write(header.Void2_1);
        writer.Write(header.Void2_2);
        writer.Write(header.NumberSamplesPlusDividedSampleRate);
        writer.Write(header.FixedValue);
    }

    /// <summary>
    /// Encodes the message to a data URI.
    /// </summary>
    public string EncodeToUri()
    {
        return DataUriPrefix + Convert.ToBase64String(EncodeToBinary());
    }
}
