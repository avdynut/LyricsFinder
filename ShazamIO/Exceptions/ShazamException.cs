namespace ShazamIO.Exceptions;

/// <summary>
/// Base exception for Shazam recognition errors.
/// </summary>
public class ShazamException : Exception
{
    public ShazamException() { }
    public ShazamException(string message) : base(message) { }
    public ShazamException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when the recognition response is not JSON.
/// </summary>
public class FailedDecodeJsonException : ShazamException
{
    public FailedDecodeJsonException() : base("Failed to decode JSON response") { }
    public FailedDecodeJsonException(string message) : base(message) { }
    public FailedDecodeJsonException(string message, Exception innerException) : base(message, innerException) { }
}
