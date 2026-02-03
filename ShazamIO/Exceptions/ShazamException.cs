namespace ShazamIO.Exceptions;

/// <summary>
/// Base exception for ShazamIO errors.
/// </summary>
public class ShazamException : Exception
{
    public ShazamException() { }
    public ShazamException(string message) : base(message) { }
    public ShazamException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when JSON decoding fails.
/// </summary>
public class FailedDecodeJsonException : ShazamException
{
    public FailedDecodeJsonException() : base("Failed to decode JSON response") { }
    public FailedDecodeJsonException(string message) : base(message) { }
    public FailedDecodeJsonException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a city name is invalid or not found.
/// </summary>
public class BadCityNameException : ShazamException
{
    public BadCityNameException() : base("City not found") { }
    public BadCityNameException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when a country name is invalid or not found.
/// </summary>
public class BadCountryNameException : ShazamException
{
    public BadCountryNameException() : base("Country not found") { }
    public BadCountryNameException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when a region name is invalid or not found.
/// </summary>
public class BadRegionNameException : ShazamException
{
    public BadRegionNameException() : base("Region not found") { }
    public BadRegionNameException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when an invalid HTTP method is used.
/// </summary>
public class BadMethodException : ShazamException
{
    public BadMethodException() : base("Invalid HTTP method") { }
    public BadMethodException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when data parsing fails.
/// </summary>
public class BadParseDataException : ShazamException
{
    public BadParseDataException() : base("Failed to parse data") { }
    public BadParseDataException(string message) : base(message) { }
}
