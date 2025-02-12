namespace backend.DTOs.Audit;

public class AuditRequestDto
{
    public string? Search { get; set; }
    public string? EventType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageOffSet { get; set; } = 0;
    public int PageSize { get; set; } = 10;
}