using ShopApi.Constants;

namespace ShopApi.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    public string Role { get; set; } = Roles.Customer;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Favorite> Favorites { get; set; } = new();

    public Cart? Cart { get; set; }

    public List<Address> Addresses { get; set; } = new();

    public List<Order> Orders { get; set; } = new();

    public List<OrderStatusHistory> OrderStatusChanges
    { get; set; } = new();

    public List<Review> Reviews { get; set; } = new();
}