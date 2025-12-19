# Docker Sandbox Execution

This document describes the sandboxed code execution system using Docker containers for secure isolation.

```mermaid
---
title: Docker Sandbox Execution Flow
---

sequenceDiagram
    participant Runner as SandboxedCodeRunner
    participant Sandbox as DockerSandbox
    participant Docker as Docker Client
    participant Engine as Docker Engine
    participant Container as Container

    Runner->>Runner: Get language config
    Runner->>Runner: Generate filename (GUID + extension)
    Runner->>Runner: Prepare code (add prefix)
    Runner->>Runner: Write code to temp file
    
    Runner->>Sandbox: ExecuteAsync(SandboxRequest)
    
    Sandbox->>Docker: CreateContainerAsync()
    Note over Docker: Image: language-specific<br/>Memory limit<br/>CPU quota<br/>No network<br/>Mount code file
    
    Docker->>Engine: Create container
    Engine-->>Docker: Container ID
    Docker-->>Sandbox: Container created
    
    Sandbox->>Docker: AttachContainerAsync()
    Docker-->>Sandbox: Stream (stdin/stdout/stderr)
    
    Sandbox->>Docker: StartContainerAsync()
    Docker->>Engine: Start container
    Engine->>Container: Run
    
    loop For each input line
        Sandbox->>Container: Write to stdin
    end
    Sandbox->>Container: Close stdin
    
    Container->>Container: Execute code
    Container-->>Sandbox: stdout/stderr
    
    Sandbox->>Docker: WaitContainerAsync()
    Docker->>Engine: Wait for exit
    Engine-->>Docker: Exit code
    Docker-->>Sandbox: Exit code
    
    Sandbox-->>Runner: SandboxResult
    
    Runner->>Runner: Delete temp file
    Runner-->>Runner: Return CodeRunnerResult
    
    Note over Sandbox: Container auto-removed<br/>or force removed in finally
```

## Sandbox Architecture

```mermaid
flowchart TB
    subgraph Host["??? Host Machine"]
        App["CodeQuiz Backend"]
        TempDir["Temp Directory<br/>/tmp/code/"]
        DockerClient["Docker Client"]
    end
    
    subgraph DockerEngine["?? Docker Engine"]
        subgraph Container["Isolated Container"]
            Runtime["Language Runtime<br/>(dotnet/python)"]
            Sandbox["/sandbox/"]
            CodeFile["script.cs"]
        end
        
        Constraints["Security Constraints"]
    end
    
    App -->|"Create container"| DockerClient
    DockerClient -->|"API calls"| DockerEngine
    TempDir -->|"Bind mount<br/>(read-only)"| Sandbox
    
    Constraints -->|"Memory: 128MB"| Container
    Constraints -->|"CPU: 50%"| Container
    Constraints -->|"Timeout: 10s"| Container
    Constraints -->|"No network"| Container
```

## Container Configuration

### SandboxRequest

```csharp
public record SandboxRequest
{
    public required string DockerImage { get; init; }
    public required string Command { get; init; }
    public required string[] Arguments { get; init; }
    public required string CodeFilePath { get; init; }
    public required string ContainerWorkDir { get; init; }
    public List<string> Input { get; init; } = [];
    public int TimeoutSeconds { get; init; } = 10;
    public long MemoryLimitBytes { get; init; } = 128 * 1024 * 1024;  // 128MB
    public long CpuQuota { get; init; } = 50000;  // 50% of one CPU
}
```

### Container Creation Parameters

```csharp
new CreateContainerParameters
{
    Image = request.DockerImage,
    Cmd = [request.Command, "/app/script.cs"],
    HostConfig = new HostConfig
    {
        Memory = request.MemoryLimitBytes + 256 * 1024 * 1024,
        CPUQuota = request.CpuQuota,
        Binds = [$"{request.CodeFilePath}:/app/script.cs"],
        AutoRemove = true
    }
}
```

## Language Configurations

### C# Configuration

```csharp
["CSharp"] = new LanguageSandboxConfig
{
    DockerImage = "mcr.microsoft.com/dotnet/sdk:10.0",
    Command = "dotnet",
    FileExtension = ".cs",
    ArgumentTemplate = ["run", "/sandbox/{filename}"],
    CodePrefix = "#pragma warning disable\n"
}
```

### Python Configuration

```csharp
["Python"] = new LanguageSandboxConfig
{
    DockerImage = "python:3.12-slim",
    Command = "python",
    FileExtension = ".py",
    ArgumentTemplate = ["/sandbox/{filename}"]
}
```

## Security Constraints

```mermaid
flowchart LR
    subgraph Constraints["?? Security Constraints"]
        Memory["Memory Limit<br/>128MB + 256MB buffer"]
        CPU["CPU Quota<br/>50% of one core"]
        Time["Timeout<br/>10 seconds"]
        Network["Network<br/>Disabled"]
        Storage["Storage<br/>No persistent writes"]
        Mount["File Mount<br/>Single code file only"]
    end
    
    subgraph Container["Container"]
        Code["User Code"]
    end
    
    Constraints --> Container
```

| Constraint | Default Value | Purpose |
|------------|---------------|---------|
| Memory | 128MB (+256MB buffer) | Prevent memory exhaustion |
| CPU Quota | 50000 (50%) | Prevent CPU monopolization |
| Timeout | 10 seconds | Prevent infinite loops |
| Network | Disabled | Prevent external calls |
| Auto-remove | Enabled | Cleanup after execution |

## SandboxedCodeRunner Flow

```mermaid
flowchart TD
    Start["RunCodeAsync(code, options)"] --> GetConfig["Get language config"]
    GetConfig --> HasConfig{Config exists?}
    
    HasConfig -->|No| Fallback["Fall back to inner runner<br/>(unsandboxed)"]
    HasConfig -->|Yes| GenFile["Generate unique filename"]
    
    GenFile --> PrepCode["Prepare code<br/>(add prefix if needed)"]
    PrepCode --> WriteFile["Write to temp file"]
    WriteFile --> Execute["Execute in sandbox"]
    
    Execute --> Success{Success?}
    Success -->|Yes| ReturnSuccess["Return success result"]
    Success -->|No| CheckTimeout{Timed out?}
    
    CheckTimeout -->|Yes| ReturnTimeout["Return timeout error"]
    CheckTimeout -->|No| ReturnError["Return error result"]
    
    Fallback --> End["Return result"]
    ReturnSuccess --> Cleanup["Delete temp file"]
    ReturnTimeout --> Cleanup
    ReturnError --> Cleanup
    Cleanup --> End
```

## Result Mapping

```csharp
return new CodeRunnerResult
{
    Success = result.Success,
    Output = result.Output,
    Error = result.Error ?? (result.TimedOut ? "Execution timed out" : null)
};
```

## Timeout Handling

The sandbox implements timeout at two levels:

1. **Container-level**: Docker engine enforces execution time
2. **Client-level**: CancellationToken with timeout + 15 seconds buffer

```csharp
using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
cts.CancelAfter(TimeSpan.FromSeconds(request.TimeoutSeconds + 15));
```

## Container Cleanup

Containers are cleaned up in multiple ways:

1. **Auto-remove**: `AutoRemove = true` in HostConfig
2. **Finally block**: Force removal if container still exists

```csharp
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
```

## Error Scenarios

| Scenario | Result |
|----------|--------|
| Execution timeout | `TimedOut = true`, Error = "Execution timed out" |
| Memory exceeded | Container killed, error returned |
| Compilation error | `Success = false`, Error contains compiler output |
| Runtime exception | `Success = false`, Error contains exception |
| Docker unavailable | Exception propagated to caller |
