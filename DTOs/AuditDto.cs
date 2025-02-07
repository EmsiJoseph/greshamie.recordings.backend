using System;
using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class AuditEntryDto
    {
        [JsonPropertyName("id")] 
        public int Id { get; set; }

        [JsonPropertyName("user")] 
        public string UserId { get; set; }

        [JsonPropertyName("event")] 
        public string EventName { get; set; } // Only event name, no ID

        [JsonPropertyName("timestamp")] 
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("details")] 
        public string? Details { get; set; }
    }
}