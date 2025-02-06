using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        public async Task SetBearerTokenAsync(HttpClient httpClientFromExternalService)
        {
            var userClaims = _httpContextAccessor.HttpContext?.User;
            if (userClaims == null)
            {
                throw new Exception("User not found in context.");
            }

            // Extract the user name
            var userName = userClaims.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userName))
            {
                throw new Exception("User name not found in token.");
            }

            // Find the user by ID instead of using GetUserAsync
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                throw new Exception($"User with ID {userName} not found.");
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
                httpClientFromExternalService.SetBearerToken(token);
            }
            catch (Exception ex)
            {
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