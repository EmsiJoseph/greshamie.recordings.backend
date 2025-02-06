using System;
using System.Threading.Tasks;
using backend.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services.Auth
{
    public class TokenService(
        HttpClient httpClient,
        IConfiguration config,
        IHttpContextAccessor httpContextAccessor,
        UserManager<User> userManager,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<TokenService> logger)
        : ITokenService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly IConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));

        private readonly UserManager<User> _userManager =
            userManager ?? throw new ArgumentNullException(nameof(userManager));

        private readonly IDataProtector _protector =
            dataProtectionProvider.CreateProtector("ClarifyGoAccessTokenProtector")
            ?? throw new ArgumentNullException(nameof(dataProtectionProvider));

        private readonly IHttpContextAccessor _httpContextAccessor =
            httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

        private readonly ILogger<TokenService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task SetBearerTokenAsync()
        {
            // Get the current user from the HTTP context.
            var userClaims = _httpContextAccessor.HttpContext?.User;
            if (userClaims == null)
            {
                throw new Exception("User not found in context.");
            }

            var user = await _userManager.GetUserAsync(userClaims);
            if (user == null)
            {
                // No user was found; do nothing.
                return;
            }

            // If the token has expired, do nothing.
            if (user.ClarifyGoAccessTokenExpiry < DateTime.UtcNow)
            {
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(user.ClarifyGoAccessToken))
                {
                    return;
                }

                // Unprotect (decrypt) the stored token.
                var token = _protector.Unprotect(user.ClarifyGoAccessToken);

                // Set the token as a Bearer token on the HttpClient.
                _httpClient.SetBearerToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting bearer token for user {UserId}", user.Id);
            }
        }


        public async Task<TokenResponse> GetAccessTokenFromClarifyGo(string username, string password)
        {
            var request = new DiscoveryDocumentRequest
            {
                Address = _config["ClarifyGoAPI:IdentityServerUri"]
                          ?? throw new Exception("IdentityServerUri is missing"),
                Policy = new DiscoveryPolicy
                {
                    ValidateIssuerName = false
                }
            };

            var discovery = await _httpClient.GetDiscoveryDocumentAsync(request);
            if (discovery.IsError)
                throw new Exception($"Discovery document request failed: {discovery.Error}");

            var response = await _httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = discovery.TokenEndpoint,
                ClientId = _config["AuthSettings:ClientId"]
                           ?? throw new Exception("AuthSettings:ClientId is missing"),
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
}