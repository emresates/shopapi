namespace ShopApi.Models;

public class Address
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string FullName { get; set; } = "";

    public string Phone { get; set; } = "";

    public string City { get; set; } = "";

    public string District { get; set; } = "";

    public string AddressLine { get; set; } = "";

    public string? PostalCode { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }

    public User User { get; set; } = null!;
}