# Code Execution System Architecture

This diagram shows the high-level architecture of the CodeQuiz code execution system.

```mermaid
---
title: CodeQuiz Code Execution System Architecture
---

flowchart TB
    subgraph MAUI_Client["??? MAUI Desktop Client"]
        subgraph Views_Client["Views"]
            CodeEditor["Code Editor<br/>(Monaco/Custom)"]
            QuizView["Quiz View"]
        end
        
        subgraph ViewModels_Client["ViewModels"]
            CreateQuizVM["CreateQuizVM"]
            JoinQuizVM["JoinQuizVM"]
        end
        
        subgraph Repositories_Client["Repositories"]
            ExecutionRepo["ExecutionRepository"]
            AttemptsRepo["AttemptsRepository"]
        end
        
        subgraph APIs_Client["API Layer (Refit)"]
            IExecutionAPI["IExecutionAPI"]
            IAttemptsAPI["IAttemptsAPI"]
        end
    end
    
    subgraph Backend["?? ASP.NET Core Backend"]
        subgraph Controllers["Controllers"]
            ExecutionCtrl["ExecutionController"]
            AttemptsCtrl["AttemptsController"]
        end
        
        subgraph Services["Execution Services"]
            CodeRunnerFactory["CodeRunnerFactory"]
            
            subgraph CodeRunners["Code Runners"]
                CSharpRunner["CSharpCodeRunner"]
                SandboxedRunner["SandboxedCodeRunner"]
            end
            
            Evaluator["Evaluator"]
            DockerSandbox["DockerSandbox"]
        end
        
        subgraph QuizServices["Quiz Services"]
            AttemptsService["AttemptsService"]
        end
    end
    
    subgraph Execution_Env["?? Execution Environment"]
        subgraph Direct["Direct Execution"]
            DotNetCLI["dotnet CLI"]
            TempFiles["Temp Code Files"]
        end
        
        subgraph Sandboxed["Docker Sandbox"]
            DockerEngine["Docker Engine"]
            Container1["Container<br/>(C# SDK)"]
            Container2["Container<br/>(Python)"]
        end
    end
    
    %% Client flow
    CodeEditor --> CreateQuizVM
    CodeEditor --> JoinQuizVM
    QuizView --> JoinQuizVM
    
    CreateQuizVM --> ExecutionRepo
    JoinQuizVM --> ExecutionRepo
    JoinQuizVM --> AttemptsRepo
    
    ExecutionRepo --> IExecutionAPI
    AttemptsRepo --> IAttemptsAPI
    
    %% API calls
    IExecutionAPI -->|"POST /run"| ExecutionCtrl
    IAttemptsAPI -->|"POST /submit"| AttemptsCtrl
    
    %% Backend flow
    ExecutionCtrl --> CodeRunnerFactory
    AttemptsCtrl --> AttemptsService
    AttemptsService --> Evaluator
    
    CodeRunnerFactory --> CSharpRunner
    CodeRunnerFactory --> SandboxedRunner
    
    Evaluator --> CodeRunnerFactory
    
    SandboxedRunner --> DockerSandbox
    
    %% Execution
    CSharpRunner --> DotNetCLI
    CSharpRunner --> TempFiles
    
    DockerSandbox --> DockerEngine
    DockerEngine --> Container1
    DockerEngine --> Container2
    
    %% Styling
    classDef clientClass fill:#e1f5fe,stroke:#01579b
    classDef backendClass fill:#f3e5f5,stroke:#4a148c
    classDef execClass fill:#e8f5e9,stroke:#2e7d32
    
    class MAUI_Client clientClass
    class Backend backendClass
    class Execution_Env execClass
```

## Component Descriptions

### MAUI Client Components

| Component | Responsibility |
|-----------|---------------|
| **Code Editor** | User interface for writing code |
| **ExecutionRepository** | Coordinates code execution API calls |
| **IExecutionAPI** | Refit interface for execution endpoints |

### Backend Components

| Component | Responsibility |
|-----------|---------------|
| **ExecutionController** | REST API endpoint for code execution |
| **CodeRunnerFactory** | Creates appropriate code runner for language |
| **CSharpCodeRunner** | Executes C# code directly via dotnet CLI |
| **SandboxedCodeRunner** | Wraps code runners with Docker isolation |
| **DockerSandbox** | Manages Docker container lifecycle |
| **Evaluator** | Runs code against test cases for grading |
| **AttemptsService** | Manages quiz attempts and triggers evaluation |

### Execution Environment

| Component | Responsibility |
|-----------|---------------|
| **dotnet CLI** | Compiles and runs C# code |
| **Docker Engine** | Container runtime for sandboxed execution |
| **Containers** | Isolated environments for code execution |

## Data Flow

1. **User writes code** in the MAUI client's code editor
2. **Run request** is sent via `IExecutionAPI` to backend
3. **ExecutionController** receives request and delegates to `CodeRunnerFactory`
4. **CodeRunnerFactory** selects appropriate runner based on language
5. **Code Runner** executes code (directly or in Docker sandbox)
6. **Result** is returned through the chain back to client

## Two Execution Paths

### Path 1: Interactive Execution
```
User ? Code Editor ? ExecutionRepository ? ExecutionController ? CodeRunner ? Result
```

### Path 2: Quiz Evaluation
```
Submit Attempt ? AttemptsService ? Evaluator ? CodeRunner ? Grade Solutions
```
