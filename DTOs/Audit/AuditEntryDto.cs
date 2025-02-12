namespace backend.DTOs.Audit
{
    public class AuditEntryDto
    {
        public int Id { get; set; }

        public string? UserName { get; set; }

        public string? EventName { get; set; }

        public string? EventType { get; set; }

        public string? RecordingId { get; set; }

        public DateTime Timestamp { get; set; }

        public string? Details { get; set; }
    }
}