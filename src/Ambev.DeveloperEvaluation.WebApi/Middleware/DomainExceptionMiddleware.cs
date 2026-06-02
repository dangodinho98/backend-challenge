using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.WebApi.Common;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

public class DomainExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public DomainExceptionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, "DomainError", ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status404NotFound, "ResourceNotFound", ex.Message);
        }
    }

    private static Task WriteErrorAsync(HttpContext context, int statusCode, string type, string detail)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            type,
            error = type,
            detail
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
