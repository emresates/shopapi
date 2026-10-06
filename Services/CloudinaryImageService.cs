using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ShopApi.Dtos.Images;
using ShopApi.Exceptions;
using ShopApi.Interfaces;

namespace ShopApi.Services;

public class CloudinaryImageService : IImageService
{
    private readonly Cloudinary _cloudinary;

    private static readonly string[] AllowedExtensions =
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private const long MaxFileSize = 5 * 1024 * 1024;

    public CloudinaryImageService(
        IConfiguration configuration
    )
    {
        var cloudName =
            configuration["Cloudinary:CloudName"];

        var apiKey =
            configuration["Cloudinary:ApiKey"];

        var apiSecret =
            configuration["Cloudinary:ApiSecret"];

        if (
            string.IsNullOrWhiteSpace(cloudName) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(apiSecret)
        )
        {
            throw new InvalidOperationException(
                "Cloudinary configuration is missing."
            );
        }

        var account = new Account(
            cloudName,
            apiKey,
            apiSecret
        );

        _cloudinary = new Cloudinary(account);
    }

    public async Task<ImageUploadResultDto> UploadAsync(
        IFormFile file
    )
    {
        ValidateFile(file);

        await using var stream =
            file.OpenReadStream();

        var uploadParams =
            new ImageUploadParams
            {
                File =
                    new FileDescription(
                        file.FileName,
                        stream
                    ),

                Folder = "shop/products",

                UseFilename = false,

                UniqueFilename = true,

                Overwrite = false
            };

        var result =
            await _cloudinary.UploadAsync(
                uploadParams
            );

        if (
            result.Error != null ||
            string.IsNullOrWhiteSpace(result.SecureUrl?.ToString()) ||
            string.IsNullOrWhiteSpace(result.PublicId)
        )
        {
            throw new AppException(
                result.Error?.Message
                    ?? "Fotoğraf yüklenemedi.",
                500,
                "imageUploadFailed"
            );
        }

        return new ImageUploadResultDto
        {
            ImageUrl =
                result.SecureUrl.ToString(),

            PublicId =
                result.PublicId
        };
    }

    public async Task DeleteAsync(
        string publicId
    )
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return;
        }

        var deleteParams =
            new DeletionParams(publicId)
            {
                ResourceType =
                    ResourceType.Image
            };

        var result =
            await _cloudinary.DestroyAsync(
                deleteParams
            );

        if (
            result.Result != "ok" &&
            result.Result != "not found"
        )
        {
            throw new AppException(
                "Fotoğraf silinemedi.",
                500,
                "imageDeleteFailed"
            );
        }
    }

    private static void ValidateFile(
        IFormFile file
    )
    {
        if (file == null || file.Length == 0)
        {
            throw new AppException(
                "Fotoğraf dosyası zorunludur.",
                400,
                "imageIsRequired"
            );
        }

        if (file.Length > MaxFileSize)
        {
            throw new AppException(
                "Fotoğraf en fazla 5 MB olabilir.",
                400,
                "imageTooLarge"
            );
        }

        var extension =
            Path.GetExtension(file.FileName)
                .ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            throw new AppException(
                "Desteklenmeyen fotoğraf formatı.",
                400,
                "imageFormatInvalid"
            );
        }
    }
}