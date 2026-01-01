# CodeQuiz

CodeQuiz is a comprehensive platform designed for conducting coding assessments and quizzes. It provides a robust environment for examiners to create technical quizzes and for examinees to solve coding challenges in a real-time environment.

The solution consists of a high-performance backend API built with ASP.NET Core and a cross-platform desktop client developed using .NET MAUI.

## Features

### For Examiners

- **Quiz Management**: Create, update, and delete quizzes.
- **Question Bank**: Add coding questions with specific requirements and test cases.
- **Configuration**: Set quiz duration, passing marks, and other settings.
- **Review**: View student attempts and grades.

### For Examinees

- **Quiz Participation**: Join quizzes using unique codes or invitations.
- **Code Execution**: Write and run code directly within the application.
- **Real-time Feedback**: Receive immediate feedback on code compilation and execution.
- **History**: View past quiz attempts and performance.

### General

- **Secure Authentication**: User registration, login, and role-based access control.
- **Profile Management**: Update user profiles and manage account settings.

## Architecture

The project is structured into two main components:

- **CodeQuizBackend**: A RESTful API built with ASP.NET Core that handles data persistence, authentication, and code execution logic. It utilizes Docker for secure and isolated code execution.
- **CodeQuizDesktop**: A .NET MAUI application that provides a modern, responsive user interface for both examiners and examinees.

## Technologies Used

### Backend

- **Framework**: ASP.NET Core (.NET 10.0)
- **Database**: MySQL with Entity Framework Core
- **Authentication**: ASP.NET Core Identity & JWT Bearer Tokens
- **Containerization**: Docker (for code execution environments)
- **Documentation**: Swagger/OpenAPI

### Desktop Client

- **Framework**: .NET MAUI (.NET 9.0)
- **Libraries**:
  - CommunityToolkit.Maui
  - Refit (Type-safe REST library)
  - Sharpnado.MaterialFrame

## Getting Started

### Prerequisites

- .NET SDK (compatible with the project versions)
- Docker Desktop (required for the backend to execute code)
- MySQL Server

### Installation

1.  **Clone the repository**

    ```bash
    git clone https://github.com/Ahmed-moh-Afifi/CodeQuiz.git
    cd CodeQuiz
    ```

2.  **Backend Setup**

    - Navigate to the backend directory: `cd CodeQuizBackend`
    - Configure your database connection string in `appsettings.json` or create a `.env` file.
    - Apply database migrations:
      ```bash
      dotnet ef database update
      ```
    - Run the backend service:
      ```bash
      dotnet run
      ```

3.  **Desktop App Setup**
    - Navigate to the desktop app directory: `cd CodeQuizDesktop`
    - Build and run the application using Visual Studio or the CLI:
      ```bash
      dotnet build
      dotnet run
      ```

## Download

The latest version of the CodeQuiz Desktop application can be downloaded from the **Releases** section of this repository.

## Documentation

For more detailed information about the system design and workflows, please refer to the `docs` folder, which contains:

- Use Case Diagrams
- Sequence Diagrams
- Class Diagrams
