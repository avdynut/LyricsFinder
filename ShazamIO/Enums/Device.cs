namespace ShazamIO.Enums;

/// <summary>
/// Device types used in Shazam API requests.
/// </summary>
public enum Device
{
    IPhone,
    Android,
    Web
}

public static class DeviceExtensions
{
    private static readonly Random Random = new();

    public static string ToValue(this Device device) => device switch
    {
        Device.IPhone => "iphone",
        Device.Android => "android",
        Device.Web => "web",
        _ => throw new ArgumentOutOfRangeException(nameof(device))
    };

    public static Device RandomDevice()
    {
        var values = Enum.GetValues<Device>();
        return values[Random.Next(values.Length)];
    }
}
