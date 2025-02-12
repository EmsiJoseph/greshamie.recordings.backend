namespace backend.DTOs
{
    public class PaginationDto
    {
        private int _pageSize = 10;
        private int _pageOffset;

        public int PageOffset
        {
            get => _pageOffset;
            set => _pageOffset = value < 0 ? 0 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : value > 50 ? 50 : value;
        }
    }
}
