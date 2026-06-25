using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            DuplicateId => (StatusCodes.Status409Conflict, "Duplicate Id"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message
        }, cancellationToken);

        return true;
    }
}
