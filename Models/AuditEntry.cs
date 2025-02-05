using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class AuditEntry
{
    public int Id { get; set; }
    [Required] public string? UserId { get; set; }
    [Required] public string? Action { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? Details { get; set; }
    [ForeignKey("UserId")] public virtual User? User { get; set; }
}