using API.Exceptions;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text.Json;

namespace API.Middleware;

public class ExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.ContentType = "application/json";

        HttpStatusCode statusCode;
        string message;

        switch (ex)
        {
            case ArgumentException:
                statusCode = HttpStatusCode.BadRequest;
                message = ex.Message;
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = ex.Message;
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = ex.Message;
                break;

            case SecurityTokenExpiredException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "Token has expired";
                break;

            case SecurityTokenException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "Invalid token";
                break;

            case ForbidException:
                statusCode = HttpStatusCode.Forbidden;
                message = ex.Message;
                break;

            case Exception:
                statusCode = HttpStatusCode.BadRequest;
                message = "An unexpected error occurred";
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred";
                break;
        }

        httpContext.Response.StatusCode = (int)statusCode;

        var response = new
        {
            StatusCode = (int)statusCode,
            Message = message,
            Error = ex.GetType().Name 
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
