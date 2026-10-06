using ShopApi.Models;

namespace ShopApi.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(User user);
}