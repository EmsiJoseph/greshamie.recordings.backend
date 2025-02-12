using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class PagedResponseDto<T>
    {
        [JsonPropertyName("items")] public IEnumerable<T> Items { get; set; } = new List<T>();

        [JsonPropertyName("pageOffSet")] public int? PageOffset { get; set; }
        [JsonPropertyName("pageSize")] public int? PageSize { get; set; }
        [JsonPropertyName("totalPages")] public int TotalPages { get; set; }
        [JsonPropertyName("totalCount")] public int TotalCount { get; set; }
        [JsonPropertyName("hasPrevious")] public bool HasPrevious { get; set; }
        [JsonPropertyName("hasNext")] public bool HasNext { get; set; }
    }
}