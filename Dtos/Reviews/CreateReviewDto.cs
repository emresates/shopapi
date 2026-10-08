using System.ComponentModel.DataAnnotations;

namespace ShopApi.Dtos.Reviews;

public class CreateReviewDto
{
    [Range(1, 5, ErrorMessage = "ratingInvalid")]
    public int Rating { get; set; }

    [MaxLength(1000, ErrorMessage = "commentTooLong")]
    public string? Comment { get; set; }
}