# CodeQuiz Authentication System Documentation

This folder contains comprehensive documentation for the CodeQuiz authentication system, covering both the ASP.NET Core backend and .NET MAUI client.

## Overview

The CodeQuiz authentication system uses **JWT (JSON Web Tokens)** with **refresh tokens** for secure authentication between the .NET MAUI client and ASP.NET Core backend.

## Documentation Files

| File | Description |
|------|-------------|
| [architecture.md](./architecture.md) | High-level system architecture diagram |
| [login-flow.md](./login-flow.md) | Detailed login sequence diagram |
| [token-refresh-flow.md](./token-refresh-flow.md) | Token refresh mechanism |
| [startup-flow.md](./startup-flow.md) | App startup authentication check |
| [data-models.md](./data-models.md) | Authentication data models and relationships |
| [jwt-structure.md](./jwt-structure.md) | JWT token structure and validation |

## Key Components

| Layer | Backend | MAUI Client |
|-------|---------|-------------|
| **Controller/API** | `AuthenticationController` | `IAuthAPI` (Refit) |
| **Service** | `JWTAuthenticationService`, `TokenService` | `TokenService`, `AuthHandler` |
| **Repository** | `UsersRepository` | `AuthenticationRepository` |
| **Identity** | ASP.NET Core Identity with `UserManager<User>` | - |
| **Storage** | MySQL (Users, RefreshTokens) | `ISecureStorage` (Platform secure storage) |

## Authentication Features

- **Login**: Username/password authentication returning JWT + refresh token
- **Register**: Create new user account via ASP.NET Core Identity
- **Token Refresh**: Automatic refresh when access token expires (client checks 5-min buffer)
- **Password Reset**: Direct reset (authenticated) or email-based reset token flow
- **Logout**: Clears stored tokens from secure storage

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/Authentication/Register` | POST | Register a new user |
| `/api/Authentication/Login` | POST | Authenticate and get tokens |
| `/api/Authentication/Refresh` | POST | Refresh access token |
| `/api/Authentication/ResetPassword` | PUT | Reset password (authenticated) |
| `/api/Authentication/ForgetPasswordRequest` | POST | Request password reset email |
| `/api/Authentication/ResetPasswordTn` | PUT | Reset password with email token |

## Security Considerations

1. **JWT Tokens** are signed with HMAC-SHA256
2. **Refresh Tokens** are stored in the database and can be revoked
3. **Client tokens** are stored in platform-specific secure storage
4. **Token expiry** is configurable via application settings
5. **Password hashing** is handled by ASP.NET Core Identity
