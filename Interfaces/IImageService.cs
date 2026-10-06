using Microsoft.AspNetCore.Http;
using ShopApi.Dtos.Images;

namespace ShopApi.Interfaces;

public interface IImageService
{
    Task<ImageUploadResultDto> UploadAsync(
        IFormFile file
    );

    Task DeleteAsync(
        string publicId
    );
}