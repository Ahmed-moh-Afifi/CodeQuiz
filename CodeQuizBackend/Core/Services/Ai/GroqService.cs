using CodeQuizBackend.Core.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace CodeQuizBackend.Core.Services.Ai
{
    /// <summary>
    /// Service for interacting with the Groq LLM API.
    /// Implements rate limiting with exponential backoff.
    /// </summary>
    public class GroqService : IGroqService
    {
        private readonly HttpClient _httpClient;
        private readonly GroqSettings _settings;
        private readonly IAppLogger<GroqService> _logger;
        private readonly SemaphoreSlim _rateLimitSemaphore = new(1, 1);
        private DateTime _lastRequestTime = DateTime.MinValue;
        private int _consecutiveRateLimitErrors = 0;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };

        public GroqService(HttpClient httpClient, IOptions<GroqSettings> settings, IAppLogger<GroqService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            // BaseAddress must end with '/' for relative URLs to work correctly
            var baseUrl = _settings.BaseUrl.TrimEnd('/') + "/";
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        }

        public async Task<string> ChatCompletionAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
        {
            var messages = new List<GroqMessage>
            {
                new() { Role = "system", Content = systemPrompt },
                new() { Role = "user", Content = userPrompt }
            };

            var response = await ChatCompletionAsync(messages, cancellationToken);
            return response.Choices.FirstOrDefault()?.Message.Content ?? string.Empty;
        }

        public async Task<GroqChatResponse> ChatCompletionAsync(List<GroqMessage> messages, CancellationToken cancellationToken = default)
        {
            await _rateLimitSemaphore.WaitAsync(cancellationToken);
            try
            {
                // Implement basic rate limiting (Groq free tier: 30 req/min)
                var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
                var minDelay = TimeSpan.FromSeconds(2); // ~30 req/min

                // Add exponential backoff if we've hit rate limits
                if (_consecutiveRateLimitErrors > 0)
                {
                    var backoffDelay = TimeSpan.FromSeconds(Math.Pow(2, _consecutiveRateLimitErrors) * 2);
                    minDelay = backoffDelay > TimeSpan.FromMinutes(2) ? TimeSpan.FromMinutes(2) : backoffDelay;
                }

                if (timeSinceLastRequest < minDelay)
                {
                    await Task.Delay(minDelay - timeSinceLastRequest, cancellationToken);
                }

                var request = new GroqChatRequest
                {
                    Model = _settings.Model,
                    Messages = messages,
                    Temperature = _settings.Temperature,
                    MaxTokens = _settings.MaxTokens
                };

                _logger.LogInfo($"Sending chat completion request to Groq (model: {_settings.Model})");
                _lastRequestTime = DateTime.UtcNow;

                var response = await _httpClient.PostAsJsonAsync("chat/completions", request, JsonOptions, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _consecutiveRateLimitErrors++;
                    _logger.LogWarning($"Rate limited by Groq API. Consecutive errors: {_consecutiveRateLimitErrors}");
                    throw new GroqRateLimitException($"Rate limited by Groq API. Will retry with backoff.");
                }

                response.EnsureSuccessStatusCode();
                _consecutiveRateLimitErrors = 0; // Reset on success

                var result = await response.Content.ReadFromJsonAsync<GroqChatResponse>(JsonOptions, cancellationToken)
                    ?? throw new GroqApiException("Failed to deserialize Groq response");

                _logger.LogInfo($"Groq response received. Tokens used: {result.Usage.TotalTokens}");
                return result;
            }
            finally
            {
                _rateLimitSemaphore.Release();
            }
        }
    }

    /// <summary>
    /// Exception thrown when Groq API rate limits are hit.
    /// </summary>
    public class GroqRateLimitException : Exception
    {
        public GroqRateLimitException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when Groq API returns an error.
    /// </summary>
    public class GroqApiException : Exception
    {
        public GroqApiException(string message) : base(message) { }
    }
}
