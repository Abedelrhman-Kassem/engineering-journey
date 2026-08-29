using System.Diagnostics;

namespace Host.Middlewares;

public static class  RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
    
}

public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path;

        if (IsSwaggerOrScalarRequest(context))
        {
            await next(context);
            return;
        }

        if (context.Request.Path.StartsWithSegments("/health"))
        {
            try
            {
                await next(context);
            }
            finally
            {
                if (context.Response.StatusCode == 200)
                {
                    logger.LogInformation("Health Check Status Code: {StatusCode}", context.Response.StatusCode);
                } 
                else
                {
                    logger.LogCritical("Health Check {StatusCode}", context.Response.StatusCode);
                }
            }

            return;
        }

        var elapsedTime = Stopwatch.StartNew();

        await next(context);

        var elapsedMilliseconds = elapsedTime.ElapsedMilliseconds;

        logger.LogInformation("Request: {Method} {Path} completed in {ElapsedMilliseconds} ms with status code {StatusCode}",
            method, path, elapsedMilliseconds, context.Response.StatusCode);

    }

    private static bool IsSwaggerOrScalarRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/swagger") || context.Request.Path.StartsWithSegments("/scalar");
    }
}

