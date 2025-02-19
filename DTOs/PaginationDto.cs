namespace backend.DTOs
{
    public class PaginationDto
    {
        private readonly int _pageSize = 10;
        private readonly int _pageOffset;

        public int PageOffset
        {
            get => _pageOffset;
            init => _pageOffset = value < 0 ? 0 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            init => _pageSize = value < 1 ? 10 : value > 50 ? 50 : value;
        }
    }
}