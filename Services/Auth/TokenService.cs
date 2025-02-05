using IdentityModel.Client;

namespace backend.Services.Auth;

public class TokenService(
    HttpClient  httpClient,
    IConfiguration config,
    IHttpContextAccessor contextAccessor) : ITokenService
{
    private readonly HttpClient _httpClient = httpClient ??
                                              throw new ArgumentNullException(nameof(httpClient));

    private readonly IConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));

    private readonly IHttpContextAccessor _contextAccessor =
        contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));


    public string? GetAccessTokenFromContext()
    {
        return _contextAccessor.HttpContext?.Request.Headers["Authorization"]
            .ToString().Replace("Bearer ", "");
    }

    public async Task<TokenResponse> GetAccessTokenAsync(string username, string password)
    {
        var request = new DiscoveryDocumentRequest
        {
            Address = _config["ClarifyGoAPI:IdentityServerUri"] ?? throw new Exception("IdentityServerUri is missing"),
            Policy = new DiscoveryPolicy
            {
                // RequireHttps = false
                ValidateIssuerName = false
            }
        };

        var discovery = await _httpClient.GetDiscoveryDocumentAsync(request);
        
        if (discovery.IsError)
            throw new Exception($"Discovery document request failed: {discovery.Error}");

        var response = await _httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = discovery.TokenEndpoint,
            ClientId = _config["AuthSettings:ClientId"] ?? throw new Exception("AuthSettings:ClientId is missing"),
            ClientSecret = _config["AuthSettings:ClientSecret"],
            Scope = _config["AuthSettings:Scope"],
            UserName = username,
            Password = password
        });

        if (response.IsError)
            throw new Exception($"Token request failed: {response.Error}");

        return response ?? throw new Exception("Access token is missing");
    }
}