namespace CodeQuizBackend.Execution.Services
{
    public interface IDockerSandbox
    {
        Task<SandboxResult> ExecuteAsync(SandboxRequest request, CancellationToken cancellationToken = default);
    }

    public record SandboxRequest
    {
        public required string DockerImage { get; init; }
        public required string Command { get; init; }
        public required string[] Arguments { get; init; }
        public required string CodeFilePath { get; init; }
        public required string ContainerWorkDir { get; init; }
        public List<string> Input { get; init; } = [];
        public int TimeoutSeconds { get; init; } = 10;
        public long MemoryLimitBytes { get; init; } = 128 * 1024 * 1024;
        public long CpuQuota { get; init; } = 50000;
    }

    public record SandboxResult
    {
        public bool Success { get; init; }
        public string? Output { get; init; }
        public string? Error { get; init; }
        public bool TimedOut { get; init; }
    }
}
