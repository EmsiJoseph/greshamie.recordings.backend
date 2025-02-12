namespace backend.DTOs
{
    public class PaginationDto
    {
        private int _pageSize = 10;
        private int _pageOffSet = 0;

        public int PageOffSet
        {
            get => _pageOffSet;
            set => _pageOffSet = value < 0 ? 0 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : value > 50 ? 50 : value;
        }
    }
}
