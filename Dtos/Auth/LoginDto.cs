using System.ComponentModel.DataAnnotations;

namespace ShopApi.Dtos.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "emailIsRequired")]
    [EmailAddress(ErrorMessage = "emailInvalid")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "passwordIsRequired")]
    public string Password { get; set; } = "";
}