using System.ComponentModel.DataAnnotations;

namespace ShopApi.Dtos.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "nameIsRequired")]
    [MinLength(2, ErrorMessage = "nameTooShort")]
    [MaxLength(100, ErrorMessage = "nameTooLong")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "emailIsRequired")]
    [EmailAddress(ErrorMessage = "emailInvalid")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "passwordIsRequired")]
    [MinLength(6, ErrorMessage = "passwordTooShort")]
    [MaxLength(100, ErrorMessage = "passwordTooLong")]
    public string Password { get; set; } = "";
}