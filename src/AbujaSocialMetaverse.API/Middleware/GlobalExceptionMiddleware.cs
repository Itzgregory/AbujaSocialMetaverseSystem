using AbujaSocialMetaverse.Shared.Exceptions;
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
            ConsentRequiredException =>
                (HttpStatusCode.Forbidden, "Consent Required"),

            DomainException notFound when notFound.Type == ErrorType.NotFound =>
                (HttpStatusCode.NotFound, "Not Found"),

            DomainException conflict when conflict.Type == ErrorType.Conflict =>
                (HttpStatusCode.Conflict, "Conflict"),

            DomainException unauthorized when unauthorized.Type == ErrorType.Unauthorized =>
                (HttpStatusCode.Unauthorized, "Unauthorized"),

            DomainException forbidden when forbidden.Type == ErrorType.Forbidden =>
                (HttpStatusCode.Forbidden, "Forbidden"),

            DomainException validation when validation.Type == ErrorType.Validation =>
                (HttpStatusCode.BadRequest, "Bad Request"),

            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, "Unauthorized"),

            ArgumentNullException =>
                (HttpStatusCode.BadRequest, "Bad Request"),

            ArgumentOutOfRangeException =>
                (HttpStatusCode.BadRequest, "Bad Request"),

            ArgumentException =>
                (HttpStatusCode.BadRequest, "Bad Request"),

            KeyNotFoundException =>
                (HttpStatusCode.NotFound, "Not Found"),

            InvalidOperationException =>
                (HttpStatusCode.UnprocessableEntity, "Unprocessable Entity"),

            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        context.Response.StatusCode = (int)statusCode;

        // Development: expose error code + message for debugging
        // Production: safe generic message — full detail goes to logs only
        var detail = _env.IsDevelopment()
            ? ex is DomainException domainEx
                ? $"[{domainEx.Code}] {domainEx.Message}"
                : ex.Message
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