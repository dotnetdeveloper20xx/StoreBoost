using StoreBoost.Application.Common.Models;
using StoreBoost.Application.Exceptions;

namespace StoreBoost.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (SlotNotFoundException ex)
        {
            _logger.LogWarning(ex, "Slot not found.");
            await WriteErrorResponse(context, 404, ex.Message);
        }
        catch (SlotAlreadyBookedException ex)
        {
            _logger.LogWarning(ex, "Slot already booked.");
            await WriteErrorResponse(context, 400, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
            await WriteErrorResponse(context, 500, "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<string>.FailureResult(message);
        await context.Response.WriteAsJsonAsync(response);
    }
}
