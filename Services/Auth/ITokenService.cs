using IdentityModel.Client;

namespace backend.Services.Auth
{
    public interface ITokenService
    {
        string? GetAccessTokenFromContext();
        Task<TokenResponse> GetAccessTokenAsync(string username, string password);
    }
}