using System.ComponentModel.DataAnnotations;

namespace ShopApi.Dtos.Orders;

public class CreateOrderDto
{
    [Range(1, int.MaxValue)]
    public int AddressId { get; set; }
}