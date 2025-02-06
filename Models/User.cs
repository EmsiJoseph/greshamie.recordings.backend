using Microsoft.AspNetCore.Identity;

namespace backend.Models;

public class User : IdentityUser
{
    public string? ClarifyGoAccessToken { get; set; }
    public DateTime? ClarifyGoAccessTokenExpiry { get; set; }
}