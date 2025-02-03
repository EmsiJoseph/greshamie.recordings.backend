using System.Net;

namespace backend.Exceptions;

public class ClarifyGoException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string ErrorDetails { get; }

    public ClarifyGoException(HttpStatusCode statusCode, string message, string details = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorDetails = details;
    }
}