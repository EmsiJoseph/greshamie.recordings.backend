
namespace backend.Exceptions
{
    public class ServiceException : Exception
    {
        public int StatusCode { get; }

        public ServiceException(string message, int statusCode = 500) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}