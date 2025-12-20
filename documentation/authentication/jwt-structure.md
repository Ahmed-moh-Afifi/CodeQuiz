# JWT Token Structure & Validation

This document describes the JWT (JSON Web Token) structure and validation process used in the authentication system.

```mermaid
---
title: JWT Token Structure & Validation
---

flowchart LR
    subgraph JWT_Token["JWT Access Token"]
        Header["Header<br/>alg: HS256<br/>typ: JWT"]
        Payload["Payload<br/>uid: user-id<br/>jti: unique-id<br/>iss: issuer<br/>aud: audience<br/>exp: expiry"]
        Signature["Signature<br/>HMACSHA256(header.payload, secret)"]
    end
    
    subgraph Backend_Validation["Backend Token Validation"]
        ValidateIssuer["? Validate Issuer"]
        ValidateAudience["? Validate Audience"]
        ValidateLifetime["? Validate Lifetime"]
        ValidateSigningKey["? Validate Signing Key"]
    end
    
    subgraph Client_Validation["Client Token Validation"]
        CheckExpiry["Check if expiry > now + 5min"]
        RefreshIfNeeded["Refresh if expiring soon"]
    end
    
    Header --> Payload --> Signature
    
    JWT_Token --> Backend_Validation
    JWT_Token --> Client_Validation
    
    subgraph Config["Configuration"]
        JWTKey["JWTKey (Secret)"]
        Issuer["Issuer"]
        Audience["Audience"]
        JWTExpiry["JWTExpiresInMinutes"]
        RefreshExpiry["RefreshTokenExpiresInDays"]
    end
    
    Config --> Backend_Validation
```

## JWT Token Structure

A JWT consists of three parts separated by dots (`.`):

```
header.payload.signature
```

### 1. Header

```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

- **alg**: Signing algorithm (HMAC-SHA256)
- **typ**: Token type

### 2. Payload (Claims)

```json
{
  "uid": "user-guid-here",
  "jti": "unique-token-id",
  "iss": "CodeQuizBackend",
  "aud": "CodeQuizClients",
  "exp": 1699999999,
  "iat": 1699996399,
  "nbf": 1699996399
}
```

| Claim | Description |
|-------|-------------|
| `uid` | User ID (custom claim) |
| `jti` | JWT ID - unique identifier for this token |
| `iss` | Issuer - who created the token |
| `aud` | Audience - intended recipient |
| `exp` | Expiration time (Unix timestamp) |
| `iat` | Issued at time |
| `nbf` | Not valid before time |

### 3. Signature

```
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  secret
)
```

## Token Generation (Backend)

```csharp
// CodeQuizBackend\Authentication\Services\TokenService.cs
public string GenerateAccessToken(IEnumerable<Claim> claims)
{
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(configuration["JWTKey"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Issuer = configuration["Issuer"],
        Audience = configuration["Audience"],
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.Now.AddMinutes(
            double.Parse(configuration["JWTExpiresInMinutes"]!)),
        SigningCredentials = creds
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}
```

### Claims Added on Login

```csharp
var claims = new[]
{
    new Claim("uid", user.Id),                           // User identifier
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())  // Unique token ID
};
```

## Backend Token Validation

Configured in `Program.cs`:

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Issuer"],
        ValidAudience = builder.Configuration["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWTKey"]!))
    };
});
```

### Validation Steps

```mermaid
flowchart TD
    Request["Incoming Request<br/>with Bearer Token"] --> Extract["Extract JWT from<br/>Authorization Header"]
    Extract --> Decode["Base64 Decode<br/>Header & Payload"]
    Decode --> VerifySig["Verify Signature<br/>using Secret Key"]
    
    VerifySig -->|Invalid| Reject1["401 Unauthorized"]
    VerifySig -->|Valid| CheckIssuer["Validate Issuer"]
    
    CheckIssuer -->|Invalid| Reject2["401 Unauthorized"]
    CheckIssuer -->|Valid| CheckAudience["Validate Audience"]
    
    CheckAudience -->|Invalid| Reject3["401 Unauthorized"]
    CheckAudience -->|Valid| CheckExpiry["Check Expiration"]
    
    CheckExpiry -->|Expired| Reject4["401 Unauthorized"]
    CheckExpiry -->|Valid| ExtractClaims["Extract Claims"]
    
    ExtractClaims --> SetIdentity["Set User Identity<br/>on HttpContext"]
    SetIdentity --> Continue["Continue to Controller"]
```

## Client-Side Token Validation

The MAUI client validates tokens locally before making API calls:

```csharp
// CodeQuizDesktop\Models\Authentication\TokenModel.cs
public bool IsValid()
{
    return ExpiryFromToken().Subtract(DateTime.Now).TotalMinutes > 5;
}

private DateTime ExpiryFromToken()
{
    var handler = new JwtSecurityTokenHandler();
    var jwt = handler.ReadJwtToken(AccessToken);
    return jwt.ValidTo.ToLocalTime();
}
```

### Client Validation Flow

```mermaid
flowchart TD
    Start["Need to make API call"] --> GetTokens["Get tokens from storage"]
    GetTokens --> HasTokens{Has tokens?}
    
    HasTokens -->|No| SessionExpired["Throw: Session Expired"]
    HasTokens -->|Yes| CheckExpiry["Read exp claim from JWT"]
    
    CheckExpiry --> TimeCheck{"Expiry - Now > 5 min?"}
    TimeCheck -->|Yes| UseToken["Use current token"]
    TimeCheck -->|No| Refresh["Trigger token refresh"]
    
    Refresh --> RefreshSuccess{Refresh OK?}
    RefreshSuccess -->|Yes| UseNewToken["Use new token"]
    RefreshSuccess -->|No| SessionExpired
```

## Token Validation for Refresh

When refreshing tokens, the backend validates the **expired** access token differently:

```csharp
public IEnumerable<Claim> GetPrincipalFromExpiredToken(string token)
{
    var tokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,  // ?? Lifetime NOT validated
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["Issuer"],
        ValidAudience = configuration["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["JWTKey"]!))
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

    if (securityToken is not JwtSecurityToken jwtSecurityToken || 
        !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, 
            StringComparison.InvariantCultureIgnoreCase))
        throw new SecurityTokenException("Invalid token");

    return jwtSecurityToken.Claims;
}
```

## Configuration Settings

| Setting | Description | Example Value |
|---------|-------------|---------------|
| `JWTKey` | Secret key for signing | 32+ character random string |
| `Issuer` | Token issuer identifier | `"CodeQuizBackend"` |
| `Audience` | Intended token audience | `"CodeQuizClients"` |
| `JWTExpiresInMinutes` | Access token lifetime | `60` (1 hour) |
| `RefreshTokenExpiresInDays` | Refresh token lifetime | `7` (1 week) |

## Security Best Practices

1. **Secret Key**: Use a strong, randomly generated key (minimum 256 bits)
2. **HTTPS Only**: Always transmit tokens over HTTPS
3. **Short Expiry**: Keep access token lifetime short (15-60 minutes)
4. **Refresh Rotation**: Generate new refresh token on each refresh
5. **Secure Storage**: Store tokens in platform-specific secure storage
6. **Token Revocation**: Ability to revoke refresh tokens in database

## Example Token

### Encoded

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIxMjM0NTY3OC0xMjM0LTEyMzQtMTIzNC0xMjM0NTY3ODkwYWIiLCJqdGkiOiJhYmNkZWYxMi0zNDU2LTc4OTAtYWJjZC1lZjEyMzQ1Njc4OTAiLCJpc3MiOiJDb2RlUXVpekJhY2tlbmQiLCJhdWQiOiJDb2RlUXVpekNsaWVudHMiLCJleHAiOjE3MDAwMDAwMDAsImlhdCI6MTY5OTk5NjQwMCwibmJmIjoxNjk5OTk2NDAwfQ.SIGNATURE_HERE
```

### Decoded Header

```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

### Decoded Payload

```json
{
  "uid": "12345678-1234-1234-1234-1234567890ab",
  "jti": "abcdef12-3456-7890-abcd-ef1234567890",
  "iss": "CodeQuizBackend",
  "aud": "CodeQuizClients",
  "exp": 1700000000,
  "iat": 1699996400,
  "nbf": 1699996400
}
```
