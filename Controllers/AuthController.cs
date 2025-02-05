using System.ComponentModel.DataAnnotations;
using backend.Services.Auth;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ITokenService tokenService, ILogger<AuthController> logger) : ControllerBase
{
    private readonly ITokenService
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));

    private readonly ILogger<AuthController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Validate model
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.Username == null || request.Password == null)
                return BadRequest(new { message = "Invalid request" });

            // Get token from IdentityServer
            var response = await _tokenService.GetAccessTokenAsync(
                request.Username,
                request.Password);

            var token = response.AccessToken ?? throw new Exception("Access token is missing");

            // Set HTTP-only cookie
            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(15)
            });

            return Ok(new { accessToken = token, expiresIn = response.ExpiresIn });
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
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string? Username { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}