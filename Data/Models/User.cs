using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace backend.Data.Models
{
    public class User : IdentityUser
    {
        [MaxLength(100)] public string? ClarifyGoAccessToken { get; set; }

        public DateTime? ClarifyGoAccessTokenExpiry { get; set; }

        [MaxLength(100)] public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        public ICollection<AuditEntry> AuditEntries { get; set; } = new List<AuditEntry>();
    }
}