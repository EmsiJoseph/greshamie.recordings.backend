using IdentityModel.Client;

namespace backend.Services.Auth
{
    public interface ITokenService
    {
        Task SetBearerTokenAsync();
        Task<TokenResponse> GetAccessTokenFromClarifyGo(string username, string password);
    }
}