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

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

                // Encrypt the ClarifyGo token before storing it.
                var encryptedToken = _protector.Protect(tokenResponse.AccessToken);
                var tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

                // Find or create the Identity user.
                var user = await _userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = request.Username,
                        // Store the encrypted ClarifyGo token.
                        ClarifyGoAccessToken = encryptedToken,
                        ClarifyGoAccessTokenExpiry = tokenExpiry
                    };

                    var createResult = await _userManager.CreateAsync(user, request.Password);

                    if (!createResult.Succeeded)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError,
                            new { message = "User creation failed." });
                    }
                }
                else
                {
                    // Update token info for an existing user.
                    user.ClarifyGoAccessToken = encryptedToken;
                    user.ClarifyGoAccessTokenExpiry = tokenExpiry;
                    user.RefreshToken = GenerateRefreshToken();
                    user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                    await _userManager.UpdateAsync(user);
                }
                // Generate a JWT token for our backend to return to the client.
                var jwtToken = GenerateJwtToken(user);
                await _auditService.LogAuditEntryAsync(user.Id, AuditEventTypes.UserLoggedIn,
                    "User logged in successfully.");
                return Ok(new
                {
                    user = new { user_name = user.UserName },
                    access_token = new
                    {
                        value = jwtToken.Token,
                        expires_in = jwtToken.ExpiresIn
                    },
                    refresh_token = user.RefreshToken
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

        private JwtTokenResult GenerateJwtToken(User user)
        {
            // Create claims for the token.
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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
                ExpiresIn = (int)(expires - DateTime.UtcNow).TotalSeconds
            };
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            // Retrieve the token from the Authorization header.
            // The expected format is: "Bearer {token}"
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return BadRequest(new { message = "Authorization header with Bearer token is required." });
            }

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

            // Attempt to get the user identifier from the token claims.
            // Here we assume that the user id is stored in the JWT 'sub' (subject) claim.
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null)
            {
                return BadRequest(new { message = "User identifier (sub claim) not found in token." });
            }

            string userId = userIdClaim.Value;

            // Log the logout event using your audit service and the predefined constant.
            await _auditService.LogAuditEntryAsync(userId, AuditEventTypes.UserLoggedOut, "User logged out.");

            // Optionally, if you are maintaining a token revocation list or performing cleanup, do it here.

            // Inform the client that logout was successful.
            return Ok(new { message = "Logged out successfully." });
        }
    }

    public class RefreshTokenRequest
    {
        [Required] public string RefreshToken { get; set; } = string.Empty;
    }
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

