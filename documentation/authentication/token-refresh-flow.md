# Token Refresh Flow

This diagram shows how the authentication system automatically refreshes expired tokens.

```mermaid
---
title: Token Refresh Flow
---

sequenceDiagram
    participant API as Protected API Call
    participant AuthHandler as AuthHandler
    participant TokenSvc as TokenService (Client)
    participant SecureStore as Secure Storage
    participant AuthAPI as IAuthAPI
    participant AuthCtrl as AuthenticationController
    participant JWTSvc as JWTAuthenticationService
    participant TokenSvcB as TokenService (Backend)
    participant DB as MySQL Database

    API->>AuthHandler: SendAsync(request)
    AuthHandler->>TokenSvc: GetValidTokens()
    
    TokenSvc->>TokenSvc: HasValidTokens()
    
    alt Token exists and valid (>5 min remaining)
        TokenSvc-->>AuthHandler: TokenModel
    else Token expired or expiring soon
        TokenSvc->>SecureStore: GetAsync("token-model")
        SecureStore-->>TokenSvc: Saved TokenModel
        
        TokenSvc->>AuthAPI: POST /Authentication/Refresh
        AuthAPI->>AuthCtrl: Refresh(TokenModel)
        AuthCtrl->>JWTSvc: RefreshToken(oldTokenModel)
        
        JWTSvc->>TokenSvcB: GetPrincipalFromExpiredToken(accessToken)
        TokenSvcB-->>JWTSvc: Claims (including uid)
        
        JWTSvc->>DB: Find RefreshToken by token & userId
        DB-->>JWTSvc: RefreshToken entity
        
        alt RefreshToken valid & not revoked & not expired
            JWTSvc->>TokenSvcB: GenerateAccessToken(claims)
            TokenSvcB-->>JWTSvc: New JWT Access Token
            
            JWTSvc->>TokenSvcB: GenerateRefreshToken()
            TokenSvcB-->>JWTSvc: New Refresh Token
            
            JWTSvc->>DB: Update RefreshToken entity
            DB-->>JWTSvc: Updated
            
            JWTSvc-->>AuthCtrl: New TokenModel
            AuthCtrl-->>AuthAPI: ApiResponse<TokenModel>
            AuthAPI-->>TokenSvc: New TokenModel
            
            TokenSvc->>SecureStore: SaveTokens(newTokenModel)
            TokenSvc-->>AuthHandler: New TokenModel
        else Invalid/Revoked/Expired
            JWTSvc-->>AuthCtrl: SecurityTokenException
            AuthCtrl-->>AuthAPI: Error
            AuthAPI-->>TokenSvc: null
            TokenSvc-->>AuthHandler: Exception "Session Expired"
        end
    end
    
    AuthHandler->>AuthHandler: Set Authorization: Bearer {token}
    AuthHandler->>API: Continue request with auth header
```

## How Token Refresh Works

### Client-Side Token Validation

The client-side `TokenService` checks if tokens are valid before making API calls:

```csharp
public bool IsValid()
{
    return ExpiryFromToken().Subtract(DateTime.Now).TotalMinutes > 5;
}
```

Tokens are considered "expiring" when less than **5 minutes** remain before expiry. This buffer ensures:
- API calls don't fail mid-request due to token expiry
- Proactive refresh before the token becomes invalid

### AuthHandler Integration

The `AuthHandler` is a `DelegatingHandler` that intercepts all HTTP requests to protected endpoints:

```csharp
protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request, 
    CancellationToken cancellationToken)
{
    var token = await tokenService.GetValidTokens() 
        ?? throw new Exception("Session Expired");
    
    request.Headers.Authorization = 
        new AuthenticationHeaderValue("Bearer", token.AccessToken);
    
    return await base.SendAsync(request, cancellationToken);
}
```

### Server-Side Refresh Logic

1. **Extract claims** from expired access token (lifetime validation disabled)
2. **Validate refresh token** in database:
   - Token matches
   - User ID matches
   - Not revoked
   - Not expired
3. **Generate new tokens**:
   - New JWT access token with same claims
   - New refresh token (GUID)
4. **Update database** with new refresh token

### Refresh Token Rotation

For security, refresh tokens are **rotated** on each refresh:
- Old refresh token is replaced with new one
- Prevents replay attacks with stolen refresh tokens
- Each refresh extends the session validity

## Configuration

| Setting | Description | Location |
|---------|-------------|----------|
| `JWTExpiresInMinutes` | Access token lifetime | Backend `appsettings.json` |
| `RefreshTokenExpiresInDays` | Refresh token lifetime | Backend `appsettings.json` |
| 5 minutes | Client-side refresh buffer | `TokenModel.IsValid()` |

## Error Scenarios

| Scenario | Client Behavior |
|----------|-----------------|
| Refresh token expired | Throw "Session Expired", redirect to login |
| Refresh token revoked | Throw "Session Expired", redirect to login |
| Network error during refresh | Retry or show error to user |
| Invalid access token structure | Throw security exception |

## Concurrency Handling

The client `TokenService` prevents multiple simultaneous refresh calls:

```csharp
private Task<TokenModel?>? refreshTask = null;

private async Task<TokenModel?> RefreshTokens()
{
    if (refreshTask != null)
    {
        return await refreshTask;
    }
    // ... refresh logic
}
```

This ensures only one refresh request is made even if multiple API calls trigger refresh simultaneously.
