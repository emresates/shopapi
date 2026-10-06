namespace ShopApi.Models.Responses;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();

    public PaginationMeta Pagination { get; set; } = new();
}