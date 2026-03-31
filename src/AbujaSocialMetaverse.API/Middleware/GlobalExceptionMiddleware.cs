using System.Net;
using System.Text.Json;

namespace AbujaSocialMetaverse.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception on {Method} {Path} | TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            await WriteErrorResponse(context, ex);
        }
    }

    private async Task WriteErrorResponse(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title) = ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            ArgumentNullException => (HttpStatusCode.BadRequest, "Bad Request"),
            ArgumentException => (HttpStatusCode.BadRequest, "Bad Request"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Not Found"),
            InvalidOperationException => (HttpStatusCode.UnprocessableEntity, "Unprocessable Entity"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        context.Response.StatusCode = (int)statusCode;

        // In development, expose the real error message for debugging
        // In production, return a safe generic message — full details go to logs only
        var detail = _env.IsDevelopment()
            ? ex.Message
            : "An error occurred processing your request. Please try again later.";

        var problem = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            detail,
            instance = context.Request.Path.ToString(),
            traceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}