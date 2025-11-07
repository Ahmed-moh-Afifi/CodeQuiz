using CodeQuizBackend.Core.Data.models;
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
            } // Catch specific exceptions here...
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception");
                await HandleExceptionAsync(context, 500, "Oops! Something went wrong on our end.");
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
