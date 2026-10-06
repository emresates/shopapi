namespace ShopApi.Dtos.Products;

public class ProductQueryDto
{
    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}