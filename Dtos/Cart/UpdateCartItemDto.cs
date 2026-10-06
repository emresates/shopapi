using System.ComponentModel.DataAnnotations;

namespace ShopApi.Dtos.Cart;

public class UpdateCartItemDto
{
    [Range(1, int.MaxValue, ErrorMessage = "quantityInvalid")]
    public int Quantity { get; set; }
}