using System.ComponentModel.DataAnnotations;

namespace ShopApi.Dtos.Cart;

public class AddToCartDto
{
    [Range(1, int.MaxValue, ErrorMessage = "quantityInvalid")]
    public int Quantity { get; set; } = 1;
}