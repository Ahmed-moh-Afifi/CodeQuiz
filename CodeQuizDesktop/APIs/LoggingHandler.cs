using System.Diagnostics;

namespace CodeQuizDesktop.APIs;

public class LoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Debug.WriteLine($"=== REQUEST ===");
        Debug.WriteLine($"{request.Method} {request.RequestUri}");
        
        if (request.Content != null)
        {
            var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            Debug.WriteLine($"Body: {requestBody}");
        }

        var response = await base.SendAsync(request, cancellationToken);

        Debug.WriteLine($"=== RESPONSE ===");
        Debug.WriteLine($"Status: {response.StatusCode}");
        
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        Debug.WriteLine($"Body: {responseBody}");

        return response;
    }
}