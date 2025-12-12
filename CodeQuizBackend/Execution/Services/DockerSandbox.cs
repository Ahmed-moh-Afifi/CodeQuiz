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

            try
            {
                // Create container with security constraints
                var createResponse = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = request.DockerImage,
                    Cmd = [request.Command, "/app/script.cs"],
                    //WorkingDir = request.ContainerWorkDir,
                    //NetworkDisabled = true,
                    HostConfig = new HostConfig
                    {
                        Memory = request.MemoryLimitBytes + 256 * 1024 * 1024,
                        //MemorySwap = request.MemoryLimitBytes,
                        CPUQuota = request.CpuQuota,
                        //Tmpfs = new Dictionary<string, string>
                        //{
                        //    ["/tmp"] = "rw,noexec,nosuid,size=64m"
                        //},
                        //Binds = [$"{request.CodeFilePath}:/app/script.cs"],
                        Binds = new List<string> { $"{request.CodeFilePath}:/app/script.cs" },
                        AutoRemove = true
                    }
                });

                containerId = createResponse.ID;
                logger.LogInfo($"Created sandbox container: {containerId[..12]}");

                // Attach to container streams
                using var stream = await _client.Containers.AttachContainerAsync(containerId, tty: false,
                    new ContainerAttachParameters
                    {
                        Stream = true,
                        Stdin = true,
                        Stdout = true,
                        Stderr = true
                    });

                // Start container
                await _client.Containers.StartContainerAsync(containerId, new ContainerStartParameters());

                // Write input
                foreach (var inputLine in request.Input)
                {
                    var inputBytes = Encoding.UTF8.GetBytes(inputLine + "\n");
                    await stream.WriteAsync(inputBytes, 0, inputBytes.Length, cancellationToken);
                }
                stream.CloseWrite();

                // Read output with timeout
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(request.TimeoutSeconds + 15));

                var (stdout, stderr) = await stream.ReadOutputToEndAsync(cts.Token);

                // Wait for container to exit
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
                logger.LogWarning("Sandbox execution timed out for container: {ContainerId}", containerId?[..12]);
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
                if (containerId is not null)
                {
                    try
                    {
                        await _client.Containers.RemoveContainerAsync(containerId,
                            new ContainerRemoveParameters { Force = true }, CancellationToken.None);
                    }
                    catch { /* Container may have been auto-removed */ }
                }
            }
        }
    }
}
