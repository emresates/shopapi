using System.Net;
using System.Text.Json;
using ShopApi.Exceptions;
using ShopApi.Models.Responses;

namespace ShopApi.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context
    )
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            await HandleAppExceptionAsync(
                context,
                ex
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Beklenmeyen bir hata oluştu."
            );

            await HandleUnknownExceptionAsync(
                context
            );
        }
    }

    private static async Task HandleAppExceptionAsync(
    HttpContext context,
    AppException exception
)
    {
        context.Response.StatusCode =
            exception.StatusCode;

        context.Response.ContentType =
            "application/json";

        var response =
            ApiResponse<object>.Error(
                exception.StatusCode,
                exception.Message,
                exception.ErrCode
            );

        var json =
            JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }

    private static async Task HandleUnknownExceptionAsync(
     HttpContext context
 )
    {
        context.Response.StatusCode =
            (int)HttpStatusCode.InternalServerError;

        context.Response.ContentType =
            "application/json";

        var response =
            ApiResponse<object>.Error(
                500,
                "Beklenmeyen bir hata oluştu.",
                "internalServerError"
            );

        var json =
            JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}