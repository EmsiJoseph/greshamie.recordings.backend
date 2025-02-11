using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Services.Audits;
using backend.Constants;
using backend.Data.Models;
using backend.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Azure.Core;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IDataProtector _protector;
        private readonly IConfiguration _configuration;
        private readonly IAuditService _auditService;

        public AuthController(
            ITokenService tokenService,
            ILogger<AuthController> logger,
            UserManager<User> userManager,
            IDataProtectionProvider dataProtectionProvider,
            IConfiguration configuration,
            IAuditService auditService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _protector = dataProtectionProvider.CreateProtector("ClarifyGoAccessTokenProtector");
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _auditService = auditService;
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        // Method to generate JWT token
        private JwtTokenResult GenerateJwtToken(User user, string role)
        {
            // Create claims for the token.
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new(JwtRegisteredClaimNames.NameId, user.Id),
                new(ClaimTypes.Role, role),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Get settings from configuration.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Set token expiration.
            var expires = DateTime.UtcNow.AddMinutes(60);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new JwtTokenResult
            {
                Token = tokenString,
                ExpiresIn = user.ClarifyGoAccessTokenExpiry.HasValue
                    ? (int)user.ClarifyGoAccessTokenExpiry.Value.Subtract(DateTime.UtcNow).TotalSeconds
                    : 0
            };
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check for valid username and password
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Invalid request" });
            }

            try
            {
                // Get token from ClarifyGo
                var tokenResponse = await _tokenService.GetAccessTokenFromClarifyGo(request.Username, request.Password);
                if (tokenResponse.IsError)
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Find or create the Identity user.
                var user = await _userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = request.Username
                    };

                    var createResult = await _userManager.CreateAsync(user, request.Password);

                    var userRole = RolesConstants.User;

                    await _userManager.AddToRoleAsync(user, userRole);

                    if (!createResult.Succeeded)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError,
                            new { message = "User creation failed." });
                    }
                }

                // Save ClarifyGo access token and expiry to user record
                user.ClarifyGoAccessToken = _protector.Protect(tokenResponse.AccessToken);
                user.ClarifyGoAccessTokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

                // Generate a random refresh token
                var refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(1);

                // Update user record with new access token, refresh token, and expiry
                await _userManager.UpdateAsync(user);


                var role = _userManager.GetRolesAsync(user).Result.FirstOrDefault();


                // Generate JWT token
                var jwtToken = GenerateJwtToken(user, role);

                // Log the login event using your audit service and the predefined constant.
                await _auditService.LogAuditEntryAsync(user.Id, AuditEventTypes.UserLoggedIn, "User logged in.");

                return Ok(new
                {
                    user = new { userName = user.UserName },
                    accessToken = new
                    {
                        value = jwtToken.Token,
                        expiresAt = DateTime.UtcNow.AddSeconds(jwtToken.ExpiresIn)
                    },
                    refreshToken = new
                    {
                        value = user.RefreshToken,
                        expiresAt = user.RefreshTokenExpiry
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user: {Username}", request.Username);
                return Unauthorized(new { message = "Invalid credentials" });
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required" });
            }

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null ||
                user.RefreshTokenExpiry == null ||
                user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }

            // Generate new tokens

            var role = _userManager.GetRolesAsync(user).Result.FirstOrDefault();

            var jwtToken = GenerateJwtToken(user, role);
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            // Log the refresh event using your audit service and the predefined constant.
            await _auditService.LogAuditEntryAsync(user.Id, AuditEventTypes.TokenRefreshed, "Token refreshed.");

            return Ok(new
            {
                user = new { userName = user.UserName },
                accessToken = new
                {
                    value = jwtToken.Token,
                    expiresAt = DateTime.UtcNow.AddSeconds(jwtToken.ExpiresIn)
                },
                refreshToken = new
                {
                    value = user.RefreshToken,
                    expiresAt = user.RefreshTokenExpiry
                }
            });
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            // Log request to verify it hits the method
            _logger.LogInformation("Logout request received.");

            // Retrieve the token from the Authorization header.
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogError("Authorization header missing or malformed.");
                return BadRequest(new { message = "Authorization header with Bearer token is required." });
            }

            _logger.LogInformation("Authorization header found, extracting token...");

            // Extract the token (removing the "Bearer " prefix).
            var token = authHeader.Substring("Bearer ".Length).Trim();

            // Use JwtSecurityTokenHandler to read (decode) the token without validating lifetime.
            var tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken;
            try
            {
                jwtToken = tokenHandler.ReadJwtToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError("Token parsing failed: " + ex.Message);
                return BadRequest(new { message = "Invalid token." });
            }

            _logger.LogInformation("Token parsed successfully.");

            // Attempt to get the user identifier from the token claims.
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null)
            {
                _logger.LogError("User identifier (sub claim) not found in token.");
                return BadRequest(new { message = "User identifier (sub claim) not found in token." });
            }

            string userId = userIdClaim.Value;
            _logger.LogInformation($"User ID extracted: {userId}");

            // Log the logout event using your audit service and the predefined constant.
            await _auditService.LogAuditEntryAsync(userId, AuditEventTypes.UserLoggedOut, "User logged out.");

            // Optionally, invalidate the refresh token and ClarifyGo access token if they exist in the database.
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                user.ClarifyGoAccessToken = null; // Remove the ClarifyGo Access Token
                user.ClarifyGoAccessTokenExpiry = null; // Remove the ClarifyGo Access Token Expiry
                await _userManager.UpdateAsync(user);
                _logger.LogInformation("Refresh token and ClarifyGo access token invalidated.");
            }

            _logger.LogInformation("Logout successful.");

            // Log the logout event using your audit service and the predefined constant.
            await _auditService.LogAuditEntryAsync(userId, AuditEventTypes.UserLoggedOut, "User logged out.");
            return Ok(new { message = "Logged out successfully." });
        }
    }

    public class RefreshTokenRequest
    {
        [Required] public string RefreshToken { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required] public string? Username { get; set; }
        [Required] public string? Password { get; set; }
    }

    public class JwtTokenResult
    {
        public string Token { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }
}