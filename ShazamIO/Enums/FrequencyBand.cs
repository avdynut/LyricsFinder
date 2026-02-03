namespace ShazamIO.Enums;

/// <summary>
/// Frequency band ranges in Hz used for audio fingerprinting.
/// </summary>
public enum FrequencyBand
{
    /// <summary>0-250 Hz - Nothing above 250 Hz is actually stored</summary>
    Hz0_250 = -1,
    
    /// <summary>250-520 Hz</summary>
    Hz250_520 = 0,
    
    /// <summary>520-1450 Hz</summary>
    Hz520_1450 = 1,
    
    /// <summary>1450-3500 Hz</summary>
    Hz1450_3500 = 2,
    
    /// <summary>3500-5500 Hz - Should not be used in legacy mode</summary>
    Hz3500_5500 = 3
}
