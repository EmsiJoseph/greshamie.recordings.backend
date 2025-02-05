using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using backend.Models;
using backend.Services.Auth;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        public AuthController(
            ITokenService tokenService,
            ILogger<AuthController> logger,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
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

                // Do not return the ClarifyGo token to the client.
                // Instead, store it in the Identity user record.
                var user = await _userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = request.Username,
                        Email = $"{request.Username}@example.com",
                        ClarifyGoAccessToken = tokenResponse.AccessToken,
                        ClarifyGoAccessTokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
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
                    user.ClarifyGoAccessToken = tokenResponse.AccessToken;
                    user.ClarifyGoAccessTokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
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