using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace TrueCode.CurrencyService.FinanceApi.Middlewares;

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
        catch (UnauthorizedAccessException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.Unauthorized, "Нет доступа");
        }
        catch (ValidationException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (ArgumentException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError,
                "Произошла внутренняя ошибка. Попробуйте позже.");
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode statusCode, string message)
    {
        _logger.LogError(exception, "Ошибка запроса: {Message}", exception.Message);

        var response = new
        {
            error = message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}