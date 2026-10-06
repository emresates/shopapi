namespace ShopApi.Models;

public class ProductImage
{
    public int Id { get; set; }

    public string ImageUrl { get; set; } = "";

    public string PublicId { get; set; } = "";

    public bool IsMain { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;
}