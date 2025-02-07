using Microsoft.AspNetCore.Identity;

namespace backend.Data.Models
{
    public class User : IdentityUser
    {
        public string? ClarifyGoAccessToken { get; set; }

        public DateTime? ClarifyGoAccessTokenExpiry { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }
}