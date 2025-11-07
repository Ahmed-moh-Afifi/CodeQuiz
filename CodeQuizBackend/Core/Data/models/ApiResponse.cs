namespace CodeQuizBackend.Core.Data.models
{
    public class ApiResponse<T>
    {
        public required bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}
