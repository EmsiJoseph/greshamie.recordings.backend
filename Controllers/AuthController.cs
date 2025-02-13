using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using backend.Services.Audits;
using backend.Constants;
using backend.Constants.Audit;
using backend.Data.Models;
using backend.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using backend.DTOs.Auth;

namespace backend.Controllers
{
    [ApiController]
    [ApiVersion(ApiVersionConstants.VersionString)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController(
        ITokenService tokenService,
        ILogger<AuthController> logger,
        UserManager<User> userManager,
        IDataProtectionProvider dataProtectionProvider,
        IConfiguration configuration,
        IAuditService auditService)
        : ControllerBase
    {
        private readonly ITokenService _tokenService =
            tokenService ?? throw new ArgumentNullException(nameof(tokenService));

        private readonly ILogger<AuthController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        private readonly UserManager<User> _userManager =
            userManager ?? throw new ArgumentNullException(nameof(userManager));

        private readonly IDataProtector _protector =
            dataProtectionProvider.CreateProtector("ClarifyGoAccessTokenProtector");

        private readonly IConfiguration _configuration =
            configuration ?? throw new ArgumentNullException(nameof(configuration));

        private readonly IAuditService _auditService =
            auditService ?? throw new ArgumentNullException(nameof(auditService));

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
                new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                new(JwtRegisteredClaimNames.NameId, user.Id),
                new(ClaimTypes.Role, role),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Get settings from configuration.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty));
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

        /// <summary>
        /// Log into the system with your username and password.
        /// Returns tokens needed for making other API calls.
        /// </summary>
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
                user.ClarifyGoAccessToken = _protector.Protect(tokenResponse.AccessToken ?? string.Empty);
                user.ClarifyGoAccessTokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

                // Generate a random refresh token
                var refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(1);

                // Update user record with new access token, refresh token, and expiry
                await _userManager.UpdateAsync(user);


                var role = _userManager.GetRolesAsync(user).Result.FirstOrDefault();


                // Generate JWT token
                var jwtToken = GenerateJwtToken(user, role ?? string.Empty);

                var auditEntry = new AuditEntry
                {
                    UserId = user.Id,
                    EventId = AuditEventsConstants.UserLoggedInId,
                    RecordId = null,
                    Details = "User logged in."
                };

                // Log the login event using your audit service and the predefined constant.
                await _auditService.LogAuditEntryAsync(auditEntry);

                return Ok(new LoginResponseDto
                {
                    User = new UserDto { UserName = user.UserName },
                    AccessToken = new TokenDto
                    {
                        Value = jwtToken.Token,
                        ExpiresAt = DateTime.UtcNow.AddSeconds(jwtToken.ExpiresIn)
                    },
                    RefreshToken = new TokenDto
                    {
                        Value = user.RefreshToken,
                        ExpiresAt = user.RefreshTokenExpiry
                    }
                });
            }
            catch (Exception)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }
        }

        /// <summary>
        /// Get a new access token using your refresh token.
        /// Use this when your access token expires.
        /// </summary>
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

            var jwtToken = GenerateJwtToken(user, role ?? string.Empty);
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var auditEntry = new AuditEntry
            {
                UserId = user.Id,
                EventId = AuditEventsConstants.TokenRefreshedId,
                RecordId = null,
                Details = "Token refreshed."
            };
            // Log the refresh event using your audit service and the predefined constant.
            await _auditService.LogAuditEntryAsync(auditEntry);

            return Ok(new LoginResponseDto
            {
                User = new UserDto { UserName = user.UserName },
                AccessToken = new TokenDto
                {
                    Value = jwtToken.Token,
                    ExpiresAt = DateTime.UtcNow.AddSeconds(jwtToken.ExpiresIn)
                },
                RefreshToken = new TokenDto
                {
                    Value = user.RefreshToken,
                    ExpiresAt = user.RefreshTokenExpiry
                }
            });
        }

        /// <summary>
        /// Log out of the system.
        /// This will invalidate your current tokens.
        /// </summary>
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
            catch (Exception)
            {
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

            var auditEntry = new AuditEntry
            {
                UserId = userId,
                EventId = AuditEventsConstants.UserLoggedOutId,
                RecordId = null,
                Details = "User logged out."
            };
            // Log the logout event using your audit service and the predefined constant.
            await _auditService.LogAuditEntryAsync(auditEntry);
            return Ok(new { message = "Logged out successfully." });
        }
    }

    public class RefreshTokenRequest
    {
        [Required] public string RefreshToken { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required] public string? Username { get; set; } = string.Empty;
        [Required] public string? Password { get; set; } = string.Empty;
    }

    public class JwtTokenResult
    {
        public string Token { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }
}