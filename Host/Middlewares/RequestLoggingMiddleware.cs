using System.Diagnostics;

namespace Host.Middlewares;

public static class RequestLoggingMiddlewareExtensions
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

        if (IsSwagger(context))
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
                if (context.Response.StatusCode >= 400)
                {
                    logger.LogCritical("Health Check {StatusCode}", context.Response.StatusCode);
                }
            }

            return;
        }

        var timer = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            var elapsedMilliseconds = timer.ElapsedMilliseconds;

            logger.LogInformation("Request: {Method} {Path} completed in {ElapsedMilliseconds} ms with status code {StatusCode}",
                method, path, elapsedMilliseconds, context.Response.StatusCode);
        }



    }

    private static bool IsSwagger(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/swagger");
    }
}

