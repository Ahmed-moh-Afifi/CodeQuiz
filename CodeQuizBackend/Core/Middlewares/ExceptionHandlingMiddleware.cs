using CodeQuizBackend.Core.Data.models;
using CodeQuizBackend.Core.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;

namespace CodeQuizBackend.Core.Middlewares
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (ResourceNotFoundException ex)
            {
                logger.LogWarning(ex, "Resource not found");
                await HandleExceptionAsync(context, StatusCodes.Status404NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request");
                await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (UnauthorizedException ex)
            {
                logger.LogWarning(ex, "Unauthorized");
                await HandleExceptionAsync(context, StatusCodes.Status401Unauthorized, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "InvalidOperation");
                await HandleExceptionAsync(context, StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception");
                await HandleExceptionAsync(context, StatusCodes.Status500InternalServerError, "Oops! Something went wrong on our end.");
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            var response = new ApiResponse<string>() { Success = false, Message = message };
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
