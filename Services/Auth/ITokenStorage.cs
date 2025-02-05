using IdentityModel.Client;

namespace backend.Services.Auth;

public interface ITokenStorage
{
    Task StoreTokenAsync(string sessionTokenId, TokenResponse token);
    Task<TokenResponse?> GetTokenAsync(string sessionTokenId);
    Task RemoveTokenAsync(string sessionTokenId);
}