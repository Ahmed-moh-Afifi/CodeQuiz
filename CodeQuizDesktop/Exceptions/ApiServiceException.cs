using CodeQuizDesktop.Models;
using System.Net;
using System.Text.Json;

namespace CodeQuizDesktop.Exceptions;

/// <summary>
/// Exception thrown when an API call fails. Contains the user-friendly message from the API response.
/// </summary>
public class ApiServiceException : Exception
{
    /// <summary>
    /// The HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// The user-friendly error message from the API response.
    /// </summary>
    public string UserMessage { get; }

    public ApiServiceException(string userMessage, HttpStatusCode statusCode, Exception? innerException = null)
        : base(userMessage, innerException)
    {
        UserMessage = userMessage;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Creates an ApiServiceException from a Refit.ApiException by extracting the message from the ApiResponse.
    /// </summary>
    public static ApiServiceException FromApiException(Refit.ApiException apiException)
    {
        var userMessage = ExtractApiResponseMessage(apiException);
        return new ApiServiceException(userMessage, apiException.StatusCode, apiException);
    }

    /// <summary>
    /// Creates an ApiServiceException from any exception, with a fallback message.
    /// </summary>
    public static ApiServiceException FromException(Exception exception, string fallbackMessage = "An unexpected error occurred.")
    {
        return exception switch
        {
            ApiServiceException apiServiceEx => apiServiceEx,
            Refit.ApiException apiEx => FromApiException(apiEx),
            HttpRequestException => new ApiServiceException(
                "Unable to connect to the server. Please check your internet connection.",
                HttpStatusCode.ServiceUnavailable,
                exception),
            TaskCanceledException => new ApiServiceException(
                "The request timed out. Please try again.",
                HttpStatusCode.RequestTimeout,
                exception),
            _ => new ApiServiceException(fallbackMessage, HttpStatusCode.InternalServerError, exception)
        };
    }

    private static string ExtractApiResponseMessage(Refit.ApiException apiException)
    {
        try
        {
            if (!string.IsNullOrEmpty(apiException.Content))
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(apiException.Content, options);

                if (apiResponse != null && !string.IsNullOrEmpty(apiResponse.Message))
                {
                    return apiResponse.Message;
                }
            }
        }
        catch (JsonException)
        {
            // Failed to parse response, fall back to status code message
        }

        return GetDefaultMessageForStatusCode(apiException.StatusCode);
    }

    private static string GetDefaultMessageForStatusCode(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "The request was invalid. Please check your input.",
            HttpStatusCode.Unauthorized => "You are not authorized.",
            HttpStatusCode.Forbidden => "You don't have permission to perform this action.",
            HttpStatusCode.NotFound => "The requested resource was not found.",
            HttpStatusCode.Conflict => "A conflict occurred. The resource may already exist.",
            HttpStatusCode.InternalServerError => "A server error occurred. Please try again later.",
            HttpStatusCode.ServiceUnavailable => "The service is temporarily unavailable. Please try again later.",
            _ => $"An error occurred: {statusCode}"
        };
    }
}
