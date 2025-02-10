using IdentityModel.Client;

namespace backend.Services.Auth
{
    public interface ITokenService
    {
        Task SetBearerTokenAsync(HttpClient httpClientFromExternalService);
        Task<TokenResponse> GetAccessTokenFromClarifyGo(string username, string password);
        
        Task SetBearerTokenWithPasswordAsync(string username, string password, HttpClient httpClientFromExternalService);
    }
}