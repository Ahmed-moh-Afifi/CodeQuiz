using CodeQuizBackend.Core.Logging;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CodeQuizBackend.Execution.Services
{
    public class DockerSandbox(IAppLogger<DockerSandbox> logger) : IDockerSandbox
    {
        private readonly DockerClient _client = new DockerClientConfiguration().CreateClient();

        public async Task<SandboxResult> ExecuteAsync(SandboxRequest request, CancellationToken cancellationToken = default)
        {
            string? containerId = null;

            // Host paths (where files actually exist on server)
            string hostCodeDir = Path.GetDirectoryName(request.CodeFilePath)!;
            string hostInputPath = Path.Combine(hostCodeDir, "input.txt");
            string fileName = Path.GetFileName(request.CodeFilePath);

            // Container paths (where Docker sees them)
            string containerCodePath = $"{request.ContainerWorkDir}/{fileName}";
            string containerInputPath = $"{request.ContainerWorkDir}/input.txt";

            try
            {
                // Create input file...
                // Even if input is empty, we create an empty file so the "<" operator doesn't crash
                var inputContent = string.Join("\n", request.Input);
                await File.WriteAllTextAsync(hostInputPath, inputContent, cancellationToken);

                // Construct shell command...
                // We join the command + arguments + input redirection
                // Example Result: "python -u script.py < input.txt"
                // Example Result: "dotnet run < input.txt"
                var argsString = string.Join(" ", request.Arguments);
                var shellCommand = $"{request.Command} {argsString} < {containerInputPath}";

                // Create container
                var createResponse = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = request.DockerImage,
                    // Use /bin/sh to enable the redirection operator "<"
                    Cmd = new List<string> { "/bin/sh", "-c", shellCommand },

                    WorkingDir = request.ContainerWorkDir,
                    AttachStdout = true,
                    AttachStderr = true,
                    Tty = false,

                    HostConfig = new HostConfig
                    {
                        Memory = request.MemoryLimitBytes,
                        CPUQuota = request.CpuQuota,
                        // Mount the specific code file and the input file
                        Binds = new List<string>
                        {
                            $"{request.CodeFilePath}:{containerCodePath}",
                            $"{hostInputPath}:{containerInputPath}"
                        },
                        AutoRemove = true
                    }
                });

                containerId = createResponse.ID;
                logger.LogInfo($"Created sandbox container: {containerId[..12]} for {fileName}");

                // Attach container (read only)
                using var stream = await _client.Containers.AttachContainerAsync(containerId, tty: false,
                    new ContainerAttachParameters
                    {
                        Stream = true,
                        Stdout = true,
                        Stderr = true
                    });

                // Start
                await _client.Containers.StartContainerAsync(containerId, new ContainerStartParameters());

                // Read output
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(request.TimeoutSeconds + 15)); // Buffer for startup (dotnet SDK image requires those extra 15 seconds)

                var (stdout, stderr) = await stream.ReadOutputToEndAsync(cts.Token);

                // Wait for exit
                var waitResponse = await _client.Containers.WaitContainerAsync(containerId, cts.Token);

                return new SandboxResult
                {
                    Success = waitResponse.StatusCode == 0,
                    Output = waitResponse.StatusCode == 0 ? stdout.Trim() : null,
                    Error = waitResponse.StatusCode != 0 ? (string.IsNullOrEmpty(stderr) ? stdout : stderr).Trim() : null,
                    TimedOut = false
                };
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Sandbox execution timed out: {ContainerId}", containerId?[..12]);
                return new SandboxResult
                {
                    Success = false,
                    Error = "Execution timed out",
                    TimedOut = true
                };
            }
            catch (Exception ex)
            {
                logger.LogError("Sandbox execution failed", ex);
                throw;
            }
            finally
            {
                // Cleanup the temporary input file
                if (File.Exists(hostInputPath))
                {
                    try { File.Delete(hostInputPath); } catch { }
                }

                // Cleanup Container (Double check in case AutoRemove failed)
                if (containerId is not null)
                {
                    try
                    {
                        await _client.Containers.RemoveContainerAsync(containerId,
                            new ContainerRemoveParameters { Force = true }, CancellationToken.None);
                    }
                    catch { }
                }
            }
        }
    }
}