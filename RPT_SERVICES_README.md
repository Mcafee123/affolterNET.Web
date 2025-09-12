# affolterNET.Auth.Core - RPT Services

This library provides **RPT (Request Party Token) Services** for enterprise-grade Keycloak authorization integration.

## 🔧 Components Included

### **Core Components:**

#### **1. Configuration**
- `AuthConfiguration` - Unified authentication configuration for OIDC and Keycloak
- `OptionsBase` - Base class for configuration options with dictionary-based property storage
- `CacheOptions` - Cache configuration settings for token and permission caching
- `RptOptions` - RPT-specific configuration including audience and caching settings

#### **2. Services**
- `RptTokenService` - Requests and manages Request Party Tokens from Keycloak authorization server
- `RptCacheService` - In-memory caching of RPT tokens with configurable expiration  
- `AuthClaimsService` - Enriches user claims with permissions extracted from RPT tokens
- `TokenHelper` - JWT token decoding and manipulation utilities

#### **3. Middleware**
- `RptMiddleware` - Automatically enriches authenticated user claims with Keycloak permissions

#### **4. Extensions**
- `AddRptServices()` - Registers all RPT services with dependency injection
- `UseRptMiddleware()` - Adds RPT middleware to the ASP.NET Core pipeline

## 🚀 Usage

### 1. Register services in your ASP.NET Core project:

```csharp
// Program.cs
builder.Services.AddRptServices(builder.Configuration);

// Add to middleware pipeline
app.UseAuthentication();
app.UseRptMiddleware(); // Adds automatic permission enrichment
app.UseAuthorization();
```

### 2. Configuration (appsettings.json):

```json
{
  "Auth": {
    "AuthorityBase": "https://your-keycloak-instance",
    "Realm": "your-realm",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "Scopes": "openid profile email",
    "CallbackPath": "/signin-oidc",
    "SignoutCallBack": "/signout-callback-oidc",
    "CookieName": ".AspNetCore.Cookies",
    "Cache": {
      "DefaultExpiration": "00:15:00",
      "PermissionCacheExpiration": "00:10:00",
      "MaxCacheSize": 1000
    },
    "Rpt": {
      "Audience": "",
      "EnableCaching": true,
      "CacheExpiration": "00:10:00"
    }
  }
}
```

### 3. Access enriched user permissions:

```csharp
[HttpGet]
[Authorize]
public IActionResult SecureEndpoint()
{
    // User claims are automatically enriched with permissions from Keycloak RPT
    var permissions = User.Claims
        .Where(c => c.Type == "permission")
        .Select(c => c.Value);
    
    return Ok(new { Permissions = permissions });
}
```

## 🔐 How It Works

1. **User Authentication**: User authenticates via OpenID Connect with Keycloak
2. **Middleware Activation**: `RptMiddleware` detects authenticated requests
3. **RPT Token Request**: `RptTokenService` requests authorization token from Keycloak
4. **Permission Extraction**: `AuthClaimsService` extracts permissions from RPT JWT
5. **Claims Enrichment**: User's `ClaimsPrincipal` is enriched with permission claims
6. **Caching**: `RptCacheService` caches tokens to improve performance

## 🏗️ Service Architecture

```
Request Flow:
HTTP Request → Authentication → RptMiddleware → AuthClaimsService → Authorization
                                      ↓              ↓
                                RptTokenService → RptCacheService
                                      ↓
                                Keycloak Server
```

### Service Dependencies:
- **RptMiddleware** → AuthClaimsService
- **AuthClaimsService** → RptTokenService + RptCacheService  
- **RptTokenService** → IKeycloakClient + AuthConfiguration
- **RptCacheService** → IMemoryCache + TokenHelper + AuthConfiguration

## 📦 Current Dependencies

- **Keycloak.NETCore.Client** (1.0.2) - Keycloak REST API client
- **System.IdentityModel.Tokens.Jwt** (8.3.0) - JWT token handling
- **Microsoft.Extensions.Caching.Abstractions** (9.0.9) - Caching infrastructure
- **Microsoft.Extensions.Options** (9.0.9) - Configuration binding
- **Microsoft.AspNetCore.App** (Framework Reference) - ASP.NET Core integration

## 🔄 Implementation Status

This RPT implementation provides:

- ✅ **Production-ready Keycloak integration** via REST API client
- ✅ **Automatic permission-based claims enrichment** through middleware
- ✅ **Performance-optimized token caching** with configurable expiration
- ✅ **Thread-safe cache operations** using semaphore locking
- ✅ **Comprehensive error handling** with graceful degradation
- ✅ **Unified configuration system** supporting OIDC and RPT settings
- ✅ **Dependency injection integration** with ASP.NET Core

## � Configuration Requirements

**Required Keycloak Setup:**
- Authorization Services enabled on your Keycloak client
- Client configured with appropriate scopes and permissions
- Resources and policies defined in Keycloak for permission-based access

**Application Configuration:**
- `Auth:AuthorityBase` - Your Keycloak server base URL
- `Auth:Realm` - Keycloak realm name  
- `Auth:ClientId` - OIDC client identifier
- `Auth:ClientSecret` - Client secret (for confidential clients)

---

**Note**: This library requires a Keycloak instance with authorization services properly configured to function correctly.