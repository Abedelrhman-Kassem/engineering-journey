using System.Diagnostics;

namespace Host.Middlewares;

public static class  RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
    
}

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (IsSwaggerOrScalarRequest(context))
        {
            await next(context);
            return;
        }

        if (context.Request.Path.StartsWithSegments("/health"))
        {
            if (context.Response.StatusCode == 200)
            {
                logger.LogInformation("Health Check Status Code: {StatusCode}", context.Response.StatusCode);
            } 
            else
            {
                logger.LogCritical("Health Check {StatusCode}", context.Response.StatusCode);
            }
            await next(context);
        }

        var elapsedTime = Stopwatch.StartNew();

        logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
        await next(context);
        logger.LogInformation("Status Code: {StatusCode}", context.Response.StatusCode);

        var elapsedMilliseconds = elapsedTime.ElapsedMilliseconds;
        logger.LogInformation("Elapsed Time: {ElapsedMilliseconds} ms", elapsedMilliseconds);

    }

    private static bool IsSwaggerOrScalarRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/swagger") || context.Request.Path.StartsWithSegments("/scalar");
    }
}

