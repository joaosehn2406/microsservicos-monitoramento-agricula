namespace BackendApi.Exceptions;

/// <summary>Base for "resource not found" errors, answered with 404.</summary>
public abstract class NotFoundException(string message) : Exception(message);
