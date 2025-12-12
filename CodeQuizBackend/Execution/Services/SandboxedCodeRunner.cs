using CodeQuizBackend.Core.Logging;
using CodeQuizBackend.Execution.Exceptions;
using CodeQuizBackend.Execution.Models;

namespace CodeQuizBackend.Execution.Services
{
    public class SandboxedCodeRunner(ICodeRunner innerRunner, IDockerSandbox sandbox, SandboxConfiguration config, IAppLogger<SandboxedCodeRunner> logger) : ICodeRunner
    {
        public string Language => innerRunner.Language;

        public async Task<CodeRunnerResult> RunCodeAsync(string code, CodeRunnerOptions? options = null)
        {
            options ??= new CodeRunnerOptions();

            // Get language-specific configuration
            if (!config.LanguageConfigs.TryGetValue(Language, out var langConfig))
            {
                logger.LogWarning("No sandbox config for {Language}, falling back to inner runner (unsandboxed)", Language); // This is only for testing and should be removed in production.
                return await innerRunner.RunCodeAsync(code, options);
            }

            string codeFileName = $"{Guid.NewGuid()}{langConfig.FileExtension}";
            string filePath = Path.Combine(config.TempCodePath, codeFileName);

            try
            {
                // Prepare the code file
                string preparedCode = langConfig.PrepareCode(code);
                await File.WriteAllTextAsync(filePath, preparedCode);

                // Execute in sandbox
                var result = await sandbox.ExecuteAsync(new SandboxRequest
                {
                    DockerImage = langConfig.DockerImage,
                    Command = langConfig.Command,
                    Arguments = langConfig.GetArguments(codeFileName),
                    CodeFilePath = filePath,
                    ContainerWorkDir = "/sandbox",
                    Input = options.Input,
                    TimeoutSeconds = config.TimeoutSeconds,
                    MemoryLimitBytes = config.MemoryLimitBytes
                });

                return new CodeRunnerResult
                {
                    Success = result.Success,
                    Output = result.Output,
                    Error = result.Error ?? (result.TimedOut ? "Execution timed out" : null)
                };
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to execute {Language} code in sandbox", ex);
                throw new CodeRunnerException($"Failed to execute {Language} code");
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch
                    {
                        logger.LogWarning($"Failed to delete temp file: {filePath}");
                    }
                }
            }
        }
    }
}
