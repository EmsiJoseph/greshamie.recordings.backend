using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Data.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using backend.Exceptions;

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
            try
            {
                var userClaims = _httpContextAccessor.HttpContext?.User;
                if (userClaims == null)
                {
                    throw new ServiceException("User not found in context", 401);
                }

                var userName = userClaims.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(userName))
                {
                    throw new ServiceException("User name not found in token", 401);
                }

                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    throw new ServiceException($"User not found: {userName}", 401);
                }

                if (user.ClarifyGoAccessTokenExpiry < DateTime.UtcNow)
                {
                    throw new ServiceException("Access token has expired", 401);
                }

                try
                {
                    if (string.IsNullOrEmpty(user.ClarifyGoAccessToken))
                    {
                        throw new ServiceException("No access token available", 401);
                    }

                    var token = _protector.Unprotect(user.ClarifyGoAccessToken);
                    httpClientFromExternalService.SetBearerToken(token);
                    
                }
                catch (Exception ex)
                {
                    throw new ServiceException($"Token processing error: {ex.Message}", 500);
                }
            }
            catch (ServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Unexpected error: {ex.Message}");
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

            Console.WriteLine("Token: " + response.AccessToken);
            return response ?? throw new Exception("Access token is missing");
        }

        public async Task SetBearerTokenWithPasswordAsync(string username, string password, HttpClient httpClientFromExternalService)
        {
            try
            {
                var tokenResponse = await GetAccessTokenFromClarifyGo(username, password);
                httpClientFromExternalService.SetBearerToken(tokenResponse.AccessToken);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting the bearer token.");
                throw;
            }
        }
    }
}