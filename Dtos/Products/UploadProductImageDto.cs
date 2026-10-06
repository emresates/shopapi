using System.ComponentModel.DataAnnotations;

namespace ShopApi.Dtos.Products;

public class UploadProductImageDto
{
    [Required]
    public IFormFile File { get; set; } = null!;

    public bool IsMain { get; set; }
}