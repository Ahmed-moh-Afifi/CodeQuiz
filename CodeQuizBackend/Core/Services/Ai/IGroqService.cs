namespace CodeQuizBackend.Core.Services.Ai
{
    /// <summary>
    /// Interface for interacting with the Groq LLM API.
    /// </summary>
    public interface IGroqService
    {
        /// <summary>
        /// Sends a chat completion request to Groq and returns the response content.
        /// </summary>
        /// <param name="systemPrompt">The system prompt to set context.</param>
        /// <param name="userPrompt">The user's message/query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The assistant's response content.</returns>
        Task<string> ChatCompletionAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a chat completion request with full control over messages.
        /// </summary>
        /// <param name="messages">The list of messages in the conversation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The full response from Groq.</returns>
        Task<GroqChatResponse> ChatCompletionAsync(List<GroqMessage> messages, CancellationToken cancellationToken = default);
    }
}
