namespace Analytics.Exceptions;

/// <summary>
/// Permanent failure: the message can never be processed (invalid JSON, missing field,
/// value out of domain). It goes straight to the DLQ with x-reprocessable=false.
/// </summary>
public sealed class InvalidMessageException : Exception
{
    public InvalidMessageException(string message)
        : base(message)
    {
    }

    public InvalidMessageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
