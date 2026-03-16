namespace criacao_api4.Dtos;

public sealed class PagedResult<T>
{
    public int pageNumber { get; set; }
    public int pageSize { get; set; }
    public int totalCount { get; set; }
    public int totalPages { get; set; }
    public List<T> items { get; set; } = new();

    public bool hasPrevious => pageNumber > 1;
    public bool hasNext => totalPages > 0 && pageNumber < totalPages;
}

