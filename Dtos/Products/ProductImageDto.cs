namespace ShopApi.Dtos.Products;

public class ProductImageDto
{
    public int Id { get; set; }

    public string ImageUrl { get; set; } = "";

    public bool IsMain { get; set; }
}