using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using backend.Models;

namespace backend.Middleware
{
    public class ClarifyGoTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        private readonly IDataProtector _protector;

        public ClarifyGoTokenHandler(
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager,
            IDataProtectionProvider dataProtectionProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            // Create a protector instance for your ClarifyGo token.
            _protector = dataProtectionProvider.CreateProtector("ClarifyGoAccessTokenProtector");
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Check if the current request has an authenticated user.
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.User?.Identity?.IsAuthenticated == true)
            {
                // Retrieve the current Identity user.
                var user = await _userManager.GetUserAsync(httpContext.User);
                if (user != null && !string.IsNullOrEmpty(user.ClarifyGoAccessToken))
                {
                    try
                    {
                        // Decrypt the stored token.
                        var accessToken = _protector.Unprotect(user.ClarifyGoAccessToken);

                        // Add the token as a Bearer token to the outgoing request.
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    }
                    catch
                    {
                        // Optionally log an error if token decryption fails.
                        // If decryption fails, do not add the header.
                    }
                }
            }

            // Continue processing the request.
            return await base.SendAsync(request, cancellationToken);
        }
    }
}