using backend.Services.Audit;
using Microsoft.AspNetCore.Mvc.Filters;

public class AuditAttribute : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var auditService = context.HttpContext.RequestServices
            .GetRequiredService<IAuditService>();
        
        if (context.HttpContext.User.Identity.IsAuthenticated)
        {
            auditService.LogAction(
                context.HttpContext.User.FindFirst("sub")?.Value,
                context.ActionDescriptor.RouteValues["action"],
                context.HttpContext.Request.Path
            );
        }
    }
}