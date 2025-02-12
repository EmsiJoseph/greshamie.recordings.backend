using IdentityModel.Client;

namespace backend.Services.Auth
{
    public interface ITokenService
    {
        /// <summary>
        /// Sets up authentication for external service calls.
        /// Uses the current user's token from their session.
        /// </summary>
        Task SetBearerTokenAsync(HttpClient httpClientFromExternalService);

        /// <summary>
        /// Gets an access token from ClarifyGo using username/password.
        /// Used during the login process.
        /// </summary>
        Task<TokenResponse> GetAccessTokenFromClarifyGo(string username, string password);
        
        /// <summary>
        /// Sets up authentication using username/password.
        /// Used when we need to make service calls outside of a user session.
        /// </summary>
        Task SetBearerTokenWithPasswordAsync(string username, string password, HttpClient httpClientFromExternalService);
    }
}