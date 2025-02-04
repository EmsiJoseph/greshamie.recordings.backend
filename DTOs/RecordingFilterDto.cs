using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class RecordingFilterDto
{
    [Required]
    public string? StartDate { get; set; }

    [Required]
    public string? EndDate { get; set; }

    [Range(0, 86400)]
    public int? MinDurationSeconds { get; set; }
}