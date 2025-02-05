using System.ComponentModel.DataAnnotations;
using backend.Models;
using backend.Services.Auth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IDataProtector _protector;

        public AuthController(
            ITokenService tokenService,
            ILogger<AuthController> logger,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IDataProtectionProvider dataProtectionProvider)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _protector = dataProtectionProvider.CreateProtector("ClarifyGoAccessTokenProtector");
        }

        [HttpPost("login")]
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
                var tokenResponse = await _tokenService.GetAccessTokenAsync(request.Username, request.Password);
                if (tokenResponse.IsError)
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Encrypt the token before storing it.
                var encryptedToken = _protector.Protect(tokenResponse.AccessToken);
                var tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

                // Do not return the ClarifyGo token to the client.
                // Instead, store it in the Identity user record.
                var user = await _userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = request.Username,
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
                    await _userManager.UpdateAsync(user);
                }

                // Sign in the user via Identity
                await _signInManager.SignInAsync(user, isPersistent: false);

                return Ok(new { message = "Logged in successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user: {Username}", request.Username);
                return Unauthorized(new { message = "Invalid credentials" });
            }
        }
    }

    public class LoginRequest
    {
        [Required] public string? Username { get; set; }

        [Required] public string? Password { get; set; }
    }
}