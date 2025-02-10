using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
using backend.Services.Audits;
using backend.Constants;
using backend.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Azure.Core;
using backend.Services;
using System.Security.Cryptography;

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

        private (string Token, int ExpiresIn) GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]); // Static key from config

            // Generate additional entropy using cryptographic randomness
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            var randomKeyPart = Convert.ToBase64String(randomBytes); // Extra randomness

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Already used
            new Claim("custom_nonce", randomKeyPart) // Extra randomness
        }),
                Expires = DateTime.UtcNow.AddHours(int.Parse(_configuration["Jwt:ExpiryHours"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return (tokenHandler.WriteToken(token), (int)(tokenDescriptor.Expires.Value - DateTime.UtcNow).TotalSeconds);
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
                // Get token from ClarifyGo (replace with actual service)
                var tokenResponse = await _tokenService.GetAccessTokenFromClarifyGo(request.Username, request.Password);
                if (tokenResponse.IsError)
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Create or update user and generate JWT token
                var user = await _userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = request.Username
                    };

                    var createResult = await _userManager.CreateAsync(user, request.Password);
                    if (!createResult.Succeeded)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError,
                            new { message = "User creation failed." });
                    }
                }

                // Save ClarifyGo access token and expiry to user record
                user.ClarifyGoAccessToken = tokenResponse.AccessToken;  // Assuming tokenResponse contains the access token
                user.ClarifyGoAccessTokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);  // Assuming the expires_in field is in seconds

                // Generate a random refresh token
                var refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);  // Set refresh token expiry, for example, 30 days

                // Update user record with new access token, refresh token, and expiry
                await _userManager.UpdateAsync(user);

                // Generate JWT token
                var (jwtToken, expiresIn) = GenerateJwtToken(user);

                return Ok(new
                {
                    user = new { user_name = user.UserName },
                    access_token = new
                    {
                        value = jwtToken,
                        expires_in = expiresIn
                    },
                    refresh_token = refreshToken  // Include the refresh token in the response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user: {Username}", request.Username);
                return Unauthorized(new { message = "Invalid credentials" });
            }
        }


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
            var jwtToken = GenerateJwtToken(user);
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                access_token = jwtToken.Token,
                refresh_token = user.RefreshToken,
                expires_in = jwtToken.ExpiresIn
            });
        }
        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("Logout request received.");

            // Retrieve the token from the Authorization header.
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogError("Authorization header missing or malformed.");
                return BadRequest(new { message = "Authorization header with Bearer token is required." });
            }

            _logger.LogInformation("Authorization header found, extracting token...");

            // Extract the Bearer token (removing the "Bearer " prefix).
            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                // Extract the user ID from the `sub` claim
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
                if (userIdClaim == null)
                {
                    _logger.LogError("User ID (sub claim) not found in token.");
                    return BadRequest(new { message = "User identifier not found in token." });
                }

                string userId = userIdClaim.Value;
                _logger.LogInformation($"Extracted User ID: {userId}");

                // Find the user by ID
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogError("No user found with the extracted User ID.");
                    return Unauthorized(new { message = "Invalid user." });
                }

                _logger.LogInformation($"User found. Wiping token-related fields.");

                // Wipe the token-related values
                user.ClarifyGoAccessToken = null;
                user.ClarifyGoAccessTokenExpiry = null;
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;

                await _userManager.UpdateAsync(user);

                // Log audit entry
                await _auditService.LogAuditEntryAsync(user.Id, AuditEventTypes.UserLoggedOut, "User logged out.");

                _logger.LogInformation("User tokens invalidated successfully.");
                return Ok(new { message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing logout: {ex.Message}");
                return BadRequest(new { message = "Invalid token format." });
            }
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
