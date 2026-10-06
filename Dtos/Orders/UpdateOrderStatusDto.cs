using System.ComponentModel.DataAnnotations;

namespace ShopApi.Dtos.Orders;

public class UpdateOrderStatusDto
{
    [Required(ErrorMessage = "orderStatusIsRequired")]
    public string Status { get; set; } = "";
}