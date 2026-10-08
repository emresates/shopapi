namespace ShopApi.Dtos.Reviews;

public class ReviewSummaryDto
{
    public int ProductId { get; set; }

    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }
}