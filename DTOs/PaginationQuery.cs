namespace criacao_api4.Dtos;

public sealed class PaginationQuery
{
    public int pageNumber { get; set; } = 1;
    public int pageSize { get; set; } = 25;

    public (int PageNumber, int PageSize, int Skip) Normalize(int maxPageSize = 25)
    {
        var normalizedPageNumber = pageNumber < 1 ? 1 : pageNumber;
        var normalizedPageSize = pageSize < 1 ? 1 : pageSize > maxPageSize ? maxPageSize : pageSize;
        var skip = (normalizedPageNumber - 1) * normalizedPageSize;
        return (normalizedPageNumber, normalizedPageSize, skip);
    }
}

