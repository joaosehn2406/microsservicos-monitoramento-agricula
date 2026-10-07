namespace BackendApi.DTOs.Requests;

public class PageQuery
{
    public const int MaxPageSize = 100;

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
