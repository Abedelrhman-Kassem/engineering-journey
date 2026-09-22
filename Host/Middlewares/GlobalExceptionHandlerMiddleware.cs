using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
namespace Host.Middlewares;

public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}

public sealed class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IProblemDetailsService problemDetailsService)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
            {
                logger.LogInformation("Request was canceled by the client.");
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
                return;
            }

            logger.LogError(ex, "An unhandled exception occurred while processing the request. TraceId: {TraceId}", Activity.Current?.Id ?? context.TraceIdentifier);

            if (context.Response.HasStarted)
            {
                logger.LogWarning("The response has already started, the global exception handler will not be executed.");
                throw;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                Exception = ex,
                HttpContext = context,
                ProblemDetails = new ProblemDetails
                {
                    Type = "about:blank",
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred. Please try again later.",
                }
            });
        }
    }
}
