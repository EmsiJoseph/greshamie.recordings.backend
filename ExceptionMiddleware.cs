namespace backend;

public class ExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ClarifyGoException ex)
        {
            context.Response.StatusCode = (int)ex.StatusCode;
            await context.Response.WriteAsJsonAsync(new
            {
                error = ex.Message,
                details = ex.Details
            });
        }
        catch (Exception ex)
        {
            var response = new
            {
                error = "Internal Server Error",
                requestId = context.TraceIdentifier
            };
            
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}