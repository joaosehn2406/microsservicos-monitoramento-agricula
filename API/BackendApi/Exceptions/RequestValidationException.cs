namespace BackendApi.Exceptions;

/// <summary>Invalid request body or query, answered with 400 and the errors per field.</summary>
public sealed class RequestValidationException(IDictionary<string, string[]> errors)
    : Exception("One or more validation errors occurred.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}
