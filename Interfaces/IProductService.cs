using ShopApi.Dtos.Products;

namespace ShopApi.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync();

    Task<ProductDto> GetByIdAsync(int id);

    Task<ProductDto> CreateAsync(CreateProductDto dto);

    Task<ProductDto> UpdateAsync(
        int id,
        UpdateProductDto dto
    );

    Task DeleteAsync(int id);

    Task<ProductImageDto> UploadImageAsync(
        int productId,
        UploadProductImageDto dto
    );

    Task DeleteImageAsync(
        int productId,
        int imageId
    );

    Task<ProductImageDto> SetMainImageAsync(
        int productId,
        int imageId
    );
}
