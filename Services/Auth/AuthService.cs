using backend.Classes;
using IdentityModel.Client;
using Microsoft.Extensions.Options;

namespace backend.Services.Auth;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthSettings _authSettings;
    private readonly ApiSettings _apiSettings;

    public AuthService(
        HttpClient httpClient,
        IOptions<AuthSettings> authSettings,
        IOptions<ApiSettings> apiSettings)
    {
        _httpClient = httpClient;
        _authSettings = authSettings.Value;
        _apiSettings = apiSettings.Value;

        _httpClient.BaseAddress = new Uri(_apiSettings.IdentityServerUri);
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var discovery = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
        {
            Policy = new DiscoveryPolicy { ValidateIssuerName = false }
        });

        if (discovery.IsError) throw new Exception(discovery.Error);

        var response = await _httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = discovery.TokenEndpoint,
            ClientId = _authSettings.ClientId,
            ClientSecret = _authSettings.ClientSecret,
            Scope = _authSettings.Scope,
            UserName = "{username}",
            Password = "{password}"
        });

        if (response.IsError) throw new Exception(response.Error);
        return response.AccessToken;
    }
}