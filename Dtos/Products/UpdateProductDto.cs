using System.ComponentModel.DataAnnotations;

namespace ShopApi.Dtos.Products;

public class UpdateProductDto
{
    [Required(ErrorMessage = "productNameIsRequired")]
    [MinLength(2, ErrorMessage = "productNameTooShort")]
    [MaxLength(150, ErrorMessage = "productNameTooLong")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "productDescriptionIsRequired")]
    [MaxLength(2000, ErrorMessage = "productDescriptionTooLong")]
    public string Description { get; set; } = "";

    [Range(0.01, double.MaxValue, ErrorMessage = "productPriceInvalid")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "productStockInvalid")]
    public int Stock { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "categoryIdInvalid")]
    public int CategoryId { get; set; }
}