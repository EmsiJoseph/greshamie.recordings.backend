using System;
using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class AuditEntryDto
    {
        [JsonPropertyName("id")] 
        public int Id { get; set; }

        [JsonPropertyName("userName")] 
        public string Username { get; set; }

        [JsonPropertyName("eventName")] 
        public string EventName { get; set; }
        
        [JsonPropertyName("recordingId")]
        public string RecordingId { get; set; }

        [JsonPropertyName("timestamp")] 
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("details")] 
        public string? Details { get; set; }
    }
}