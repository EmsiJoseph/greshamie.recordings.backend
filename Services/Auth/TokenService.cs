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

    public async Task<string> GetAccessTokenAsync(string username, string password)
    {
        var discovery = await _httpClient.GetDiscoveryDocumentAsync();
        if (discovery.IsError)
            throw new Exception($"Discovery failed: {discovery.Error}");

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

        return response.AccessToken ?? throw new Exception("Access token is missing");
    }
}