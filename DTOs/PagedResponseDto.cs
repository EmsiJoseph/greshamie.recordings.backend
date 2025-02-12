namespace backend.DTOs
{
    public class PagedResponseDto<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int? PageOffset { get; set; }
        public int? PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
    }
}