using System.ComponentModel.DataAnnotations;

namespace backend.Data.Models
{
    public class CallType
    {
        [Key] public int? Id { get; set; }

        [Required] [MaxLength(50)] public string Name { get; set; } = string.Empty;

        [Required] [MaxLength(100)] public string Description { get; set; } = string.Empty;
    }
}