# CodeQuiz Code Execution System Documentation

This folder contains comprehensive documentation for the CodeQuiz code execution system, which enables secure execution of user-submitted code for quiz evaluation.

## Overview

The code execution system allows users to run code in multiple programming languages, with support for both direct execution and sandboxed Docker-based execution for security.

## Documentation Files

| File | Description |
|------|-------------|
| [architecture.md](./architecture.md) | High-level system architecture diagram |
| [code-execution-flow.md](./code-execution-flow.md) | Detailed code execution sequence |
| [sandbox-execution.md](./sandbox-execution.md) | Docker sandbox execution flow |
| [evaluation-flow.md](./evaluation-flow.md) | Quiz solution evaluation process |
| [data-models.md](./data-models.md) | Execution data models and relationships |
| [security.md](./security.md) | Security considerations and sandbox constraints |

## Key Components

| Layer | Backend | MAUI Client |
|-------|---------|-------------|
| **Controller/API** | `ExecutionController` | `IExecutionAPI` (Refit) |
| **Service** | `ICodeRunner`, `IEvaluator`, `IDockerSandbox` | - |
| **Factory** | `CodeRunnerFactory` | - |
| **Repository** | - | `ExecutionRepository` |

## Supported Languages

| Language | Docker Image | File Extension |
|----------|--------------|----------------|
| C# | `mcr.microsoft.com/dotnet/sdk:10.0` | `.cs` |
| Python | `python:3.12-slim` | `.py` |

## Execution Modes

### 1. Direct Execution
- Code runs directly on the host machine
- Uses `dotnet run` for C# code
- Fast but less secure

### 2. Sandboxed Execution (Docker)
- Code runs in isolated Docker containers
- Resource limits enforced (CPU, memory, time)
- Network disabled for security
- Recommended for production

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/Execution/run` | POST | Execute code and return output |

## Use Cases

1. **Interactive Code Editor**: Users can run code while working on quiz solutions
2. **Automatic Grading**: System evaluates solutions against test cases
3. **Practice Mode**: Users can test code without submitting

## Security Features

- Docker container isolation
- Memory limits (default: 128MB)
- CPU quota limits
- Execution timeout (default: 10 seconds)
- Network isolation
- Temporary file cleanup
- No persistent storage in containers
