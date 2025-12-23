using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace CobranzaCloud.Api.Middleware;

/// <summary>
/// Global error handling middleware following RFC 7807 Problem Details
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problemDetails) = exception switch
        {
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                CreateProblemDetails("Unauthorized", "No tiene autorización para acceder a este recurso", 401)),

            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                CreateProblemDetails("Bad Request", argEx.Message, 400)),

            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                CreateProblemDetails("Not Found", "El recurso solicitado no fue encontrado", 404)),

            _ => (
                HttpStatusCode.InternalServerError,
                CreateProblemDetails("Internal Server Error", GetErrorMessage(exception), 500))
        };

        _logger.LogError(
            exception,
            "Error processing request {Method} {Path}. Status: {StatusCode}",
            context.Request.Method,
            context.Request.Path,
            (int)statusCode);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }

    private ProblemDetails CreateProblemDetails(string title, string detail, int status)
    {
        return new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = status,
            Type = $"https://httpstatuses.com/{status}"
        };
    }

    private string GetErrorMessage(Exception exception)
    {
        // Only show detailed errors in development
        return _env.IsDevelopment()
            ? exception.Message
            : "Ha ocurrido un error interno. Por favor intente más tarde.";
    }
}

public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
