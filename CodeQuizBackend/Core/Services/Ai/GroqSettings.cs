namespace CodeQuizBackend.Core.Services.Ai
{
    /// <summary>
    /// Configuration settings for Groq API integration.
    /// Loaded from environment variables.
    /// </summary>
    public class GroqSettings
    {
        /// <summary>
        /// Groq API key for authentication.
        /// </summary>
        public required string ApiKey { get; set; }

        /// <summary>
        /// The model to use for completions (e.g., "llama-3.3-70b-versatile").
        /// </summary>
        public string Model { get; set; } = "llama-3.3-70b-versatile";

        /// <summary>
        /// Maximum tokens to generate in completions.
        /// </summary>
        public int MaxTokens { get; set; } = 2048;

        /// <summary>
        /// Temperature for response randomness (0.0 = deterministic, 1.0 = creative).
        /// </summary>
        public float Temperature { get; set; } = 0.3f;

        /// <summary>
        /// Base URL for Groq API.
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1";
    }
}
