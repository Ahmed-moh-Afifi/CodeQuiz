using CodeQuizBackend.Authentication.Exceptions;
using CodeQuizBackend.Core.Data.models;
using CodeQuizBackend.Core.Exceptions;
using CodeQuizBackend.Execution.Exceptions;
using CodeQuizBackend.Quiz.Exceptions;

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
            catch (ForbiddenException ex)
            {
                logger.LogWarning(ex, "Forbidden");
                await HandleExceptionAsync(context, StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (AuthenticationException ex)
            {
                logger.LogWarning(ex, "Authentication Exception");
                await HandleExceptionAsync(context, StatusCodes.Status401Unauthorized, ex.Message);
            }
            catch (QuizNotActiveException ex)
            {
                logger.LogWarning(ex, "Quiz not active");
                await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (MultipleAttemptsNotAllowedException ex)
            {
                logger.LogWarning(ex, "Multiple attempts not allowed");
                await HandleExceptionAsync(context, StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (AttemptNotSubmittedException ex)
            {
                logger.LogWarning(ex, "Attempt not submitted");
                await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (UnsupportedLanguageException ex)
            {
                logger.LogWarning(ex, "Unsupported language");
                await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (CodeRunnerException ex)
            {
                logger.LogError(ex, "Code runner exception");
                await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (ServiceUnavailableException ex)
            {
                logger.LogError(ex, "Service unavailable");
                await HandleExceptionAsync(context, StatusCodes.Status503ServiceUnavailable, ex.Message);
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
