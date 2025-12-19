# App Startup Authentication Flow

This diagram shows how the application determines whether to show the login page or main page on startup.

```mermaid
---
title: App Startup Authentication Check
---

sequenceDiagram
    participant App as App Startup
    participant StartupVM as StartupViewModel
    participant TokenSvc as TokenService
    participant SecureStore as Secure Storage
    participant AuthAPI as IAuthAPI
    participant UsersRepo as UsersRepository
    participant AuthRepo as AuthenticationRepository
    participant Shell as Shell Navigation

    App->>StartupVM: Initialize()
    StartupVM->>StartupVM: Wait 2 seconds (splash)
    
    StartupVM->>TokenSvc: GetValidTokens()
    TokenSvc->>SecureStore: GetAsync("token-model")
    
    alt Tokens found in storage
        SecureStore-->>TokenSvc: TokenModel JSON
        TokenSvc->>TokenSvc: Deserialize & check IsValid()
        
        alt Access Token valid (>5 min remaining)
            TokenSvc-->>StartupVM: TokenModel
        else Access Token expired/expiring
            TokenSvc->>AuthAPI: Refresh(oldTokenModel)
            
            alt Refresh successful
                AuthAPI-->>TokenSvc: New TokenModel
                TokenSvc->>SecureStore: Save new tokens
                TokenSvc-->>StartupVM: New TokenModel
            else Refresh failed
                TokenSvc-->>StartupVM: null
            end
        end
    else No tokens in storage
        SecureStore-->>TokenSvc: null
        TokenSvc-->>StartupVM: null
    end
    
    alt Valid tokens obtained
        StartupVM->>UsersRepo: GetUser()
        UsersRepo-->>StartupVM: User
        StartupVM->>AuthRepo: LoggedInUser = User
        StartupVM->>Shell: GoToAsync("///MainPage")
    else No valid tokens
        StartupVM->>Shell: GoToAsync("///LoginPage")
    end
```

## Startup Process

### StartupViewModel Implementation

```csharp
public class StartupViewModel : BaseViewModel
{
    public StartupViewModel(
        ITokenService tokenService, 
        IAuthenticationRepository authenticationRepository, 
        IUsersRepository usersRepository)
    {
        // ...
        Initialize();
    }

    public async void Initialize()
    {
        await Task.Delay(2000);  // Splash screen delay
        var token = await tokenService.GetValidTokens();
        
        if (token != null)
        {
            authenticationRepository.LoggedInUser = await usersRepository.GetUser();
            await Shell.Current.GoToAsync("///MainPage");
        }
        else
        {
            await Shell.Current.GoToAsync("///LoginPage");
        }
    }
}
```

### Decision Logic

```mermaid
flowchart TD
    Start["App Starts"] --> Splash["Show Splash Screen<br/>(2 seconds)"]
    Splash --> CheckStorage["Check Secure Storage<br/>for saved tokens"]
    
    CheckStorage -->|No tokens| Login["Navigate to Login Page"]
    CheckStorage -->|Tokens found| ValidCheck["Check if access token<br/>is valid (>5 min remaining)"]
    
    ValidCheck -->|Valid| FetchUser["Fetch user profile<br/>from backend"]
    ValidCheck -->|Expired/Expiring| Refresh["Attempt token refresh"]
    
    Refresh -->|Success| FetchUser
    Refresh -->|Failed| Login
    
    FetchUser -->|Success| Main["Navigate to Main Page"]
    FetchUser -->|Failed| Login
```

## Token Storage

Tokens are stored using platform-specific secure storage:

| Platform | Storage Mechanism |
|----------|------------------|
| **Windows** | Windows Credential Manager |
| **macOS** | Keychain |
| **iOS** | Keychain |
| **Android** | Android Keystore |

### Storage Key

```
Key: "token-model"
Value: JSON serialized TokenModel
```

Example stored value:
```json
{
    "AccessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "RefreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

## Session Restoration

When valid tokens are found:

1. **Get user profile** via `UsersRepository.GetUser()`
2. **Set logged-in user** in `AuthenticationRepository.LoggedInUser`
3. **Navigate** to the main page

This allows the app to seamlessly restore the user session without requiring re-authentication.

## Edge Cases

| Scenario | Behavior |
|----------|----------|
| First app launch | No tokens ? Login page |
| Token valid | Main page |
| Token expired, refresh valid | Refresh ? Main page |
| Token expired, refresh expired | Login page |
| Token valid, user fetch fails | Should handle gracefully (currently may cause issues) |
| Network unavailable | Refresh fails ? Login page |

## Security Considerations

1. **Secure Storage**: Platform-specific secure storage protects tokens at rest
2. **Token Validation**: Expired tokens trigger server-side validation via refresh
3. **User Verification**: User profile is fetched to ensure account still exists
4. **Session Continuity**: Users don't need to re-authenticate on every app launch
