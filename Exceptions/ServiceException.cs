
namespace backend.Exceptions
{
    public class ServiceException(string message, int statusCode = 500) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
    }
}