namespace ShazamIO.Algorithm;

/// <summary>
/// Helper class for CRC32 computation compatible with Shazam signatures.
/// </summary>
public static class Crc32Helper
{
    private static readonly uint[] Crc32Table;

    static Crc32Helper()
    {
        Crc32Table = new uint[256];
        const uint polynomial = 0xEDB88320;
        
        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (int j = 0; j < 8; j++)
            {
                crc = (crc & 1) == 1 ? (crc >> 1) ^ polynomial : crc >> 1;
            }
            Crc32Table[i] = crc;
        }
    }

    /// <summary>
    /// Computes CRC32 checksum for the given data.
    /// </summary>
    public static uint Compute(byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        
        foreach (byte b in data)
        {
            crc = (crc >> 8) ^ Crc32Table[(crc ^ b) & 0xFF];
        }
        
        return crc ^ 0xFFFFFFFF;
    }

    /// <summary>
    /// Computes CRC32 checksum for the given data with offset and length.
    /// </summary>
    public static uint Compute(byte[] data, int offset, int length)
    {
        uint crc = 0xFFFFFFFF;
        
        for (int i = offset; i < offset + length; i++)
        {
            crc = (crc >> 8) ^ Crc32Table[(crc ^ data[i]) & 0xFF];
        }
        
        return crc ^ 0xFFFFFFFF;
    }
}
