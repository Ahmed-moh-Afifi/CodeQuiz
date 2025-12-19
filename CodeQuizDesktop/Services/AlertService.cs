using CodeQuizDesktop.Exceptions;
using CodeQuizDesktop.Logging;
using CodeQuizDesktop.Models;
using System.Text.Json;

namespace CodeQuizDesktop.Services;

public class AlertService(IAppLogger<AlertService> logger) : IAlertService
{
    public async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        if (Application.Current?.Windows.Count > 0)
        {
            var mainPage = Application.Current.Windows[0].Page;
            if (mainPage != null)
            {
                await mainPage.DisplayAlert(title, message, cancel);
            }
        }
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No")
    {
        if (Application.Current?.Windows.Count > 0)
        {
            var mainPage = Application.Current.Windows[0].Page;
            if (mainPage != null)
            {
                return await mainPage.DisplayAlert(title, message, accept, cancel);
            }
        }
        return false;
    }

    public async Task ShowErrorAsync(string message, string? title = null)
    {
        logger.LogError(message);
        await ShowAlertAsync(title ?? "Error", message);
    }

    public async Task ShowErrorAsync(Exception exception, string? userFriendlyMessage = null)
    {
        logger.LogError(userFriendlyMessage ?? exception.Message, exception);
        
        var message = userFriendlyMessage ?? GetUserFriendlyMessage(exception);
        await ShowAlertAsync("Error", message);
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            ApiServiceException apiServiceEx => apiServiceEx.UserMessage,
            HttpRequestException => "Unable to connect to the server. Please check your internet connection.",
            TaskCanceledException => "The request timed out. Please try again.",
            Refit.ValidationApiException validationEx => validationEx.Content?.ToString() ?? "Validation error occurred.",
            Refit.ApiException apiEx => ApiServiceException.FromApiException(apiEx).UserMessage,
            UnauthorizedAccessException => "You are not authorized to perform this action.",
            _ => "An unexpected error occurred. Please try again."
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
            // Failed to parse response, fall back to default message
        }
        
        return $"Server error: {apiException.StatusCode}";
    }
}
