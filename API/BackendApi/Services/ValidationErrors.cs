using BackendApi.DTOs.Requests;
using BackendApi.Exceptions;

namespace BackendApi.Services;

/// <summary>Collects validation errors per field and throws them all at once.</summary>
public sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> _errors = new();

    public void Add(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var messages))
        {
            _errors[field] = messages = [];
        }

        messages.Add(message);
    }

    /// <summary>Null is accepted (optional filter or validated elsewhere).</summary>
    public void AddIfNotAllowed(string field, string? value, IReadOnlySet<string> allowed)
    {
        if (value is not null && !allowed.Contains(value))
        {
            Add(field, $"{field} must be one of: {string.Join(", ", allowed)}.");
        }
    }

    public void AddPageErrors(PageQuery query)
    {
        if (query.Page < 1) Add("page", "page must be greater than or equal to 1.");
        if (query.PageSize is < 1 or > PageQuery.MaxPageSize)
        {
            Add("pageSize", $"pageSize must be between 1 and {PageQuery.MaxPageSize}.");
        }
    }

    public void AddRangeErrors(DateTimeOffset? from, DateTimeOffset? to)
    {
        if (from is not null && to is not null && from > to) Add("from", "from must be earlier than or equal to to.");
    }

    public void ThrowIfAny()
    {
        if (_errors.Count > 0)
        {
            throw new RequestValidationException(_errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray()));
        }
    }
}
