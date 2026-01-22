namespace CodeQuizBackend.Core.Services.Ai
{
    /// <summary>
    /// Represents a chat completion request to the Groq API.
    /// </summary>
    public class GroqChatRequest
    {
        public required string Model { get; set; }
        public required List<GroqMessage> Messages { get; set; }
        public float Temperature { get; set; } = 0.3f;
        public int MaxTokens { get; set; } = 2048;
    }

    /// <summary>
    /// Represents a message in the chat conversation.
    /// </summary>
    public class GroqMessage
    {
        public required string Role { get; set; }  // "system", "user", "assistant"
        public required string Content { get; set; }
    }

    /// <summary>
    /// Represents the response from Groq API.
    /// </summary>
    public class GroqChatResponse
    {
        public required string Id { get; set; }
        public required string Object { get; set; }
        public required long Created { get; set; }
        public required string Model { get; set; }
        public required List<GroqChoice> Choices { get; set; }
        public required GroqUsage Usage { get; set; }
    }

    /// <summary>
    /// Represents a completion choice.
    /// </summary>
    public class GroqChoice
    {
        public required int Index { get; set; }
        public required GroqMessage Message { get; set; }
        public string? FinishReason { get; set; }
    }

    /// <summary>
    /// Represents token usage information.
    /// </summary>
    public class GroqUsage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}
