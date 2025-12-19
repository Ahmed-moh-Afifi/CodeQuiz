# Authentication System Architecture

This diagram shows the high-level architecture of the CodeQuiz authentication system, including both the MAUI client and ASP.NET Core backend components.

```mermaid
---
title: CodeQuiz Authentication System Architecture
---

flowchart TB
    subgraph MAUI_Client["??? MAUI Desktop Client"]
        subgraph Views["Views Layer"]
            StartupPage["Startup Page"]
            LoginPage["Login Page"]
            RegisterPage["Register Page"]
            MainPage["Main Page"]
        end
        
        subgraph ViewModels["ViewModels Layer"]
            StartupVM["StartupViewModel"]
            LoginVM["LoginVM"]
            RegisterVM["RegisterVM"]
        end
        
        subgraph Services_Client["Services"]
            TokenServiceClient["TokenService"]
            SecureStorage["ISecureStorage<br/>(Platform Secure Storage)"]
        end
        
        subgraph Repositories_Client["Repositories"]
            AuthRepo["AuthenticationRepository"]
            UsersRepo["UsersRepository"]
        end
        
        subgraph APIs["API Layer (Refit)"]
            IAuthAPI["IAuthAPI"]
            AuthHandler["AuthHandler<br/>(DelegatingHandler)"]
            OtherAPIs["IQuizzesAPI, IUsersAPI,<br/>IAttemptsAPI, IExecutionAPI"]
        end
    end
    
    subgraph Backend["?? ASP.NET Core Backend"]
        subgraph Controllers["Controllers Layer"]
            AuthController["AuthenticationController"]
            UsersController["UsersController"]
        end
        
        subgraph Services_Backend["Services Layer"]
            JWTAuthService["JWTAuthenticationService"]
            TokenServiceBackend["TokenService"]
        end
        
        subgraph Identity["ASP.NET Core Identity"]
            UserManager["UserManager&lt;User&gt;"]
            IdentityRole["IdentityRole"]
        end
        
        subgraph Middleware["Middleware & Auth"]
            JWTBearer["JWT Bearer Authentication"]
            ExceptionMiddleware["ExceptionHandlingMiddleware"]
        end
        
        subgraph Data["Data Layer"]
            DbContext["ApplicationDbContext"]
            UserEntity["User : IdentityUser"]
            RefreshTokenEntity["RefreshToken"]
        end
    end
    
    subgraph Storage["?? Storage"]
        MySQL[("MySQL Database")]
        ClientSecure[("Device Secure Storage")]
    end
    
    %% Client-side flow
    StartupPage --> StartupVM
    LoginPage --> LoginVM
    RegisterPage --> RegisterVM
    
    StartupVM --> TokenServiceClient
    LoginVM --> AuthRepo
    LoginVM --> TokenServiceClient
    RegisterVM --> AuthRepo
    
    AuthRepo --> IAuthAPI
    TokenServiceClient --> SecureStorage
    TokenServiceClient --> IAuthAPI
    SecureStorage --> ClientSecure
    
    AuthHandler --> TokenServiceClient
    OtherAPIs --> AuthHandler
    
    %% API calls
    IAuthAPI -->|"HTTP POST/PUT"| AuthController
    
    %% Backend flow
    AuthController --> JWTAuthService
    JWTAuthService --> UserManager
    JWTAuthService --> TokenServiceBackend
    JWTAuthService --> DbContext
    
    UserManager --> DbContext
    DbContext --> MySQL
    
    JWTBearer -->|"Validates JWT"| AuthController
```

## Component Descriptions

### MAUI Client Components

| Component | Responsibility |
|-----------|---------------|
| **Views** | UI pages for login, registration, and main app |
| **ViewModels** | Handle UI logic and coordinate with repositories |
| **TokenService** | Manage token storage, validation, and refresh |
| **AuthenticationRepository** | Coordinate authentication operations |
| **IAuthAPI** | Refit interface for HTTP calls to backend |
| **AuthHandler** | HTTP message handler that attaches JWT to requests |

### Backend Components

| Component | Responsibility |
|-----------|---------------|
| **AuthenticationController** | REST API endpoints for auth operations |
| **JWTAuthenticationService** | Core authentication logic |
| **TokenService** | Generate and validate JWT and refresh tokens |
| **UserManager** | ASP.NET Core Identity user management |
| **ApplicationDbContext** | Entity Framework database context |

### Data Flow

1. **Unauthenticated requests** (login, register) go directly to `IAuthAPI`
2. **Authenticated requests** pass through `AuthHandler` which:
   - Gets valid tokens from `TokenService`
   - Attaches Bearer token to request headers
3. **Backend** validates JWT via `JwtBearerAuthentication` middleware
4. **Refresh flow** is triggered automatically when tokens are expiring
