using Anyware.OrdersAPI.Application.Common.Models;
using Anyware.OrdersAPI.Domain.Exceptions;
using System.Net;
using System.Text.Json;
namespace Anyware.OrdersAPI.API.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger,
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
            var errorResponse = CreateErrorResponse(context, exception);

            LogException(exception, context);

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = errorResponse.Status;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            };

            var json = JsonSerializer.Serialize(errorResponse, options);
            await context.Response.WriteAsync(json);
        }

        private ErrorResponse CreateErrorResponse(HttpContext context, Exception exception)
        {
            var (status, title, errorCode, detail) = exception switch
            {
                BaseCustomException customEx => (
                    customEx.StatusCode,
                    GetTitleForStatusCode(customEx.StatusCode),
                    customEx.ErrorCode,
                    customEx.Message
                ),
                OperationCanceledException => (
                    499, 
                    "Request Cancelled",
                    "REQUEST_CANCELLED",
                    "The request was cancelled by the client."
                ),
                _ => (
                    (int)HttpStatusCode.InternalServerError,
                    "Internal Server Error",
                    "INTERNAL_ERROR",
                    _environment.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred. Please try again later."
                )
            };

            var errorResponse = new ErrorResponse
            {
                Type = $"https://httpstatuses.com/{status}",
                Title = title,
                Status = status,
                Detail = detail,
                Instance = context.Request.Path,
                ErrorCode = errorCode,
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            if (exception is ValidationException validationEx && validationEx.Errors.Any())
            {
                errorResponse.Errors = validationEx.Errors;
            }

            
            if (_environment.IsDevelopment() && exception is not BaseCustomException)
            {
                errorResponse.Detail = $"{exception.Message}\n\nStack Trace:{exception.StackTrace}";
            }

            return errorResponse;
        }

        private void LogException(Exception exception, HttpContext context)
        {
            var logMessage = $"Exception occurred: {exception.GetType().Name} | Path: {context.Request.Path} | TraceId: {context.TraceIdentifier}";

            switch (exception)
            {
                case NotFoundException:
                    _logger.LogWarning(exception, logMessage);
                    break;
                case ValidationException:
                    _logger.LogWarning(exception, logMessage);
                    break;
                case CacheException:
                    //Cache failures shouldn't be critical - log as warning
                    _logger.LogWarning(exception, "Cache operation failed but request continued: {Message}", exception.Message);
                    break;
                case OperationCanceledException:
                    _logger.LogInformation("Request cancelled by client: {Path}", context.Request.Path);
                    break;
                default:
                    _logger.LogError(exception, logMessage);
                    break;
            }
        }

        private static string GetTitleForStatusCode(int statusCode) => statusCode switch
        {
            400 => "Bad Request",
            404 => "Not Found",
            409 => "Conflict",
            500 => "Internal Server Error",
            _ => "Error"
        };
    }
}
