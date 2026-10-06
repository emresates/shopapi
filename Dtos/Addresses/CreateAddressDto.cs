using System.ComponentModel.DataAnnotations;

namespace ShopApi.Dtos.Addresses;

public class CreateAddressDto
{
    [Required]
    [MaxLength(50)]
    public string Title { get; set; } = "";

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = "";

    [Required]
    [MaxLength(30)]
    public string Phone { get; set; } = "";

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = "";

    [Required]
    [MaxLength(100)]
    public string District { get; set; } = "";

    [Required]
    [MaxLength(500)]
    public string AddressLine { get; set; } = "";

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    public bool IsDefault { get; set; }
}