using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Infrastructure;

/// <summary>
/// Global exception handler middleware that converts exceptions to Problem Details responses.
/// Pattern: Chain of Responsibility - handles exceptions at a central point in the pipeline.
/// </summary>
public sealed class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = MapException(exception);

        _logger.LogError(
            exception,
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, StatusCode: {StatusCode}",
            context.TraceIdentifier,
            context.Request.Path,
            statusCode);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = _environment.IsDevelopment() ? exception.Message : detail,
            Instance = context.Request.Path,
            Type = GetProblemType(statusCode)
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = new
            {
                type = exception.GetType().Name,
                message = exception.Message,
                stackTrace = exception.StackTrace
            };
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonOptions));
    }

    private static (int StatusCode, string Title, string Detail) MapException(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException ex => (
                (int)HttpStatusCode.BadRequest,
                "Invalid Request",
                $"Required parameter '{ex.ParamName}' was not provided."),

            ArgumentException ex => (
                (int)HttpStatusCode.BadRequest,
                "Invalid Request",
                ex.Message),

            InvalidOperationException ex when ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) => (
                (int)HttpStatusCode.NotFound,
                "Resource Not Found",
                ex.Message),

            InvalidOperationException ex when ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase) => (
                (int)HttpStatusCode.Conflict,
                "Resource Conflict",
                ex.Message),

            InvalidOperationException ex => (
                (int)HttpStatusCode.BadRequest,
                "Invalid Operation",
                ex.Message),

            UnauthorizedAccessException => (
                (int)HttpStatusCode.Unauthorized,
                "Unauthorized",
                "Authentication is required to access this resource."),

            KeyNotFoundException ex => (
                (int)HttpStatusCode.NotFound,
                "Resource Not Found",
                ex.Message),

            NotImplementedException => (
                (int)HttpStatusCode.NotImplemented,
                "Not Implemented",
                "This feature is not yet implemented."),

            OperationCanceledException => (
                499, // Client Closed Request
                "Request Cancelled",
                "The request was cancelled by the client."),

            TimeoutException => (
                (int)HttpStatusCode.GatewayTimeout,
                "Request Timeout",
                "The request took too long to process."),

            HttpRequestException => (
                (int)HttpStatusCode.BadGateway,
                "External Service Error",
                "An error occurred while communicating with an external service."),

            Azure.RequestFailedException ex when ex.Status == 404 => (
                (int)HttpStatusCode.NotFound,
                "Resource Not Found",
                "The requested Azure resource was not found."),

            Azure.RequestFailedException ex when ex.Status == 429 => (
                (int)HttpStatusCode.TooManyRequests,
                "Too Many Requests",
                "Azure service rate limit exceeded. Please try again later."),

            Azure.RequestFailedException => (
                (int)HttpStatusCode.BadGateway,
                "Azure Service Error",
                "An error occurred while communicating with Azure services."),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please try again later.")
        };
    }

    private static string GetProblemType(int statusCode)
    {
        return statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
            403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            429 => "https://tools.ietf.org/html/rfc6585#section-4",
            500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            501 => "https://tools.ietf.org/html/rfc7231#section-6.6.2",
            502 => "https://tools.ietf.org/html/rfc7231#section-6.6.3",
            504 => "https://tools.ietf.org/html/rfc7231#section-6.6.5",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
    }
}

/// <summary>
/// Extension methods for adding the exception handler middleware.
/// </summary>
public static class ExceptionHandlerMiddlewareExtensions
{
    /// <summary>
    /// Adds the global exception handler middleware to the application pipeline.
    /// Should be added early in the pipeline to catch all exceptions.
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}
