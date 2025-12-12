namespace CodeQuizBackend.Execution.Models;

public class SandboxConfiguration
{
    public string TempCodePath { get; set; } = "/tmp/code";
    public int TimeoutSeconds { get; set; } = 10;
    public long MemoryLimitBytes { get; set; } = 128 * 1024 * 1024;
    public Dictionary<string, LanguageSandboxConfig> LanguageConfigs { get; set; } = [];
}

public class LanguageSandboxConfig
{
    public required string DockerImage { get; set; }
    public required string Command { get; set; }
    public required string FileExtension { get; set; }
    public string[] ArgumentTemplate { get; set; } = ["{filename}"];
    public string? CodePrefix { get; set; }

    public string[] GetArguments(string filename) =>
        ArgumentTemplate.Select(arg => arg.Replace("{filename}", filename)).ToArray();

    public string PrepareCode(string code) =>
        string.IsNullOrEmpty(CodePrefix) ? code : CodePrefix + code;
}