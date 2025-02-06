using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class AuditEntry
{
    public int Id { get; set; }
    [Required] public string? UserId { get; set; }
    [Required] public string? ActionId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? Details { get; set; }
    [ForeignKey("UserId")] public virtual User? User { get; set; }
    [ForeignKey("ActionId")] public virtual AuditAction? Action { get; set; }
}