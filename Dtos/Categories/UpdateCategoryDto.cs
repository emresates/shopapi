using System.ComponentModel.DataAnnotations;

namespace ShopApi.Dtos.Categories;

public class UpdateCategoryDto
{
    [Required(ErrorMessage = "categoryNameIsRequired")]
    [MinLength(2, ErrorMessage = "categoryNameTooShort")]
    [MaxLength(100, ErrorMessage = "categoryNameTooLong")]
    public string Name { get; set; } = "";
}