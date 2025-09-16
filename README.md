# affolterNET.Auth - Authentication Modes

This library provides flexible authenticatConfigure the authentication mode in your `appsettings.json`:

```json
{
  "affolterNET.Web": {
    "Bff": {
      "Options": {
        "AuthMode": "Authenticate",
        "EnableSessionManagement": true,
        "EnableTokenRefresh": true,
        "EnableAntiforgery": true,
        "EnableHttpsRedirection": true
      }
    },
    "SecurityHeaders": {
      "Enabled": true
    }
  }
}
```zation modes for ASP.NET Core applications with YARP reverse proxy integration.

## Authentication Modes

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                               AUTHENTICATION MODES                              │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────┐  ┌─────────────────┐  ┌─────────────────────────────────────┐
│      NONE       │  │   AUTHENTICATE  │  │             AUTHORIZE               │
│                 │  │     (LOGIN)     │  │                                     │
│ Anonymous       │  │ Login Required  │  │ Login + Permission Claims Required  │
│ Access          │  │ No Permissions  │  │ Fine-grained Access Control         │
└─────────────────┘  └─────────────────┘  └─────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                            ALWAYS ENABLED SERVICES                              │
│                        (Security & Infrastructure)                              │
├─────────────────────────────────────────────────────────────────────────────────┤
│ ✅ SecurityHeadersMiddleware     │ CSP, HSTS, X-Frame-Options, etc.             │
│ ✅ AntiforgeryTokenMiddleware    │ CSRF protection                              │
│ ✅ HTTP Context Accessor         │ Core infrastructure                          │
│ ✅ Memory Cache                  │ Performance & caching                        │
│ ✅ YARP Reverse Proxy           │ Frontend/API proxying                         │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                           MODE-SPECIFIC SERVICES                                │
└─────────────────────────────────────────────────────────────────────────────────┘

MODE: NONE                    MODE: AUTHENTICATE             MODE: AUTHORIZE
├─────────────────────┐      ├─────────────────────────┐   ├──────────────────────────┐
│ Services:           │      │ Services:               │   │ Services:                │
│ • Basic Routing     │      │ • Cookie Authentication │   │ • Cookie Authentication  │
│ • Static Files      │      │ • OIDC Integration      │   │ • OIDC Integration       │
│                     │      │ • Token Refresh         │   │ • Token Refresh          │
│ Middleware:         │      │ • Claims Enrichment     │   │ • Claims Enrichment      │
│ • No Auth Pipeline  │      │ • Basic Authorization   │   │ • Permission Policies    │
│                     │      │                         │   │ • RPT Token Service      │
│ Use Cases:          │      │ Middleware:             │   │ • Keycloak Integration   │
│ • Public websites   │      │ • UseAuthentication()   │   │                          │
│ • Static content    │      │ • UseAuthorization()    │   │ Middleware:              │
│ • Development       │      │ • RefreshTokenMware     │   │ • UseAuthentication()    │
└─────────────────────┘      │ • RptMiddleware         │   │ • UseAuthorization()     │
                             │                         │   │ • RefreshTokenMware      │
                             │ Use Cases:              │   │ • RptMiddleware          │
                             │ • Internal tools        │   │                          │
                             │ • Simple apps           │   │ Use Cases:               │
                             │ • Prototyping           │   │ • Enterprise apps        │
                             └─────────────────────────┘   │ • Multi-tenant systems   │
                                                           │ • Fine-grained access    │
                                                           └──────────────────────────┘

┌────────────────────────────────────────────────────────────────────────────────┐
│                               SERVICE MATRIX                                   │
├─────────────────────────────────┬───────┬────────────────┬─────────────────────┤
│ Service/Middleware              │ NONE  │ AUTHENTICATE   │ AUTHORIZE           │
├─────────────────────────────────┼───────┼────────────────┼─────────────────────┤
│ SecurityHeadersMiddleware       │   ✅   │      ✅        │         ✅          │
│ AntiforgeryTokenMiddleware      │   ✅   │      ✅        │         ✅          │
│ HTTP Context Accessor           │   ✅   │      ✅        │         ✅          │
│ Memory Cache                    │   ✅   │      ✅        │         ✅          │
│ YARP Reverse Proxy              │   ✅   │      ✅        │         ✅          │
│ Static Files                    │   ✅   │      ✅        │         ✅          │
│ API NotFound Handling           │   ✅   │      ✅        │         ✅          │
├─────────────────────────────────┼───────┼────────────────┼─────────────────────┤
│ Cookie Authentication           │   ❌   │      ✅        │         ✅          │
│ OIDC Integration                │   ❌   │      ✅        │         ✅          │
│ UseAuthentication()             │   ❌   │      ✅        │         ✅          │
│ UseAuthorization()              │   ❌   │      ✅        │         ✅          │
│ Session Management              │   ❌   │      ✅        │         ✅          │
│ Token Refresh Middleware        │   ❌   │      ✅        │         ✅          │
│ No Unauthorized Redirect        │   ❌   │      ✅        │         ✅          │
├─────────────────────────────────┼───────┼────────────────┼─────────────────────┤
│ RPT Token Service               │   ❌   │      ❌        │         ✅          │
│ Permission Policies             │   ❌   │      ❌        │         ✅          │
│ Permission Claims Service       │   ❌   │      ❌        │         ✅          │
└─────────────────────────────────┴───────┴────────────────┴─────────────────────┘
```

## Configuration

Configure the authorization mode in your `appsettings.json`:

```json
{
  "Auth": {
    "AuthenticationMode": "Authenticate",
    "RequireHttpsMetadata": true,
    "RedirectUri": "/signin-oidc",
    "PostLogoutRedirectUri": "/",
    "Cookie": { 
      "Secure": true 
    }
  }
}
```

### Available Authentication Modes

- **`None`**: Anonymous access, no authentication required
- **`Authenticate`**: Login required, no permission checks
- **`Authorize`**: Login + fine-grained permission validation

## Usage

### 1. Register Services

```csharp
var bffOptions = builder.Services.AddBffServices(isDev, builder.Configuration, options =>
{
    options.EnableSecurityHeaders = true;
    options.ConfigureBff = bffOptions =>
    {
        bffOptions.AuthMode = AuthenticationMode.Authenticate;
        bffOptions.EnableSessionManagement = true;
        bffOptions.EnableTokenRefresh = true;
    };
});
```

### 2. Configure Middleware Pipeline

```csharp
app.ConfigureBffApp(bffOptions);
```

## Key Features

- **Progressive Enhancement**: Each mode builds upon the previous one
- **YARP Integration**: Reverse proxy works seamlessly in all modes
- **Security First**: CSP, Antiforgery, and Security Headers always enabled
- **Flexible Configuration**: Easy mode switching via configuration
- **Clean Service Registration**: Only required services are registered per mode
- **Swagger Integration**: Built-in OpenAPI documentation support
- **Multi-Section Configuration**: Separate configuration sections for different concerns

## Usage Pattern

The library follows a two-step configuration pattern:

1. **Service Registration**: `AddBffServices()` returns configuration object
2. **Pipeline Configuration**: `ConfigureBffApp()` accepts the configuration object

```csharp
// Step 1: Register services and get configuration
var bffOptions = builder.Services.AddBffServices(isDev, builder.Configuration, options => { /* configure */ });

// Step 2: Configure middleware pipeline
app.ConfigureBffApp(bffOptions);
```

## Technical Configuration Switches

The BFF library provides fine-grained control over features through configuration switches. These can be set in `appsettings.json` or programmatically:

### Core Application Switches (All Modes)
- **`EnableSecurityHeaders`**: Security headers middleware at application level (default: `true`)

### BFF-Specific Switches
- **`EnableApiNotFound`**: API 404 handling for unmatched routes (default: `true`)
- **`EnableAntiforgery`**: CSRF protection with antiforgery tokens (default: `true`) 
- **`EnableHttpsRedirection`**: HTTPS enforcement middleware (default: `true`)
- **`EnableStaticFiles`**: Static file serving capability (default: `true`)
- **`EnableYarp`**: Reverse proxy functionality (default: `true`)

### Authentication Switches (Authenticate + Authorize Modes)
- **`EnableSessionManagement`**: Session handling and management (default: `true`)
- **`EnableTokenRefresh`**: Automatic token renewal middleware (default: `true`)
- **`EnableNoUnauthorizedRedirect`**: Prevent API route redirects on 401 (default: `true`)
- **`RevokeRefreshTokenOnLogout`**: Cleanup tokens on logout (default: `true`)

### Authorization Switches (Authorize Mode Only)  
- **`EnableRptTokens`**: Resource Permission Token support (default: `true`)

### Configuration Example

```json
{
  "affolterNET.Web": {
    "Bff": {
      "Options": {
        "AuthMode": "Authorize",
        "EnableSessionManagement": true,
        "EnableTokenRefresh": true,
        "EnableRptTokens": true,
        "EnableAntiforgery": true,
        "EnableApiNotFound": true,
        "EnableStaticFiles": true,
        "EnableYarp": true,
        "EnableHttpsRedirection": false,
        "RevokeRefreshTokenOnLogout": true
      }
    }
  }
}
```

### Programmatic Configuration

```csharp
var bffOptions = builder.Services.AddBffServices(isDev, builder.Configuration, options =>
{
    // Core application options
    options.EnableSecurityHeaders = true;
    
    // BFF-specific configuration
    options.ConfigureBff = bffOptions =>
    {
        bffOptions.AuthMode = AuthenticationMode.Authorize;
        bffOptions.EnableSessionManagement = true;
        bffOptions.EnableTokenRefresh = true;
        bffOptions.EnableRptTokens = true;
        bffOptions.EnableAntiforgery = false; // Disable for APIs
        bffOptions.EnableHttpsRedirection = false; // For development
    };
    
    // Swagger/OpenAPI configuration (optional)
    options.ConfigureSwagger = swaggerOptions =>
    {
        swaggerOptions.Title = "My API";
        swaggerOptions.Version = "v1";
        swaggerOptions.ConfigureApiDocumentation = app =>
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        };
    };
});
```

## Architecture

### Core Components

- **affolterNET.Auth.Core**: Base authorization policies, middleware, and services
- **affolterNET.Auth.Bff**: Backend-for-Frontend pattern with YARP integration
- **affolterNET.Auth.Api**: API-specific authentication (if needed)

### Security Services (Always Active)

- **SecurityHeadersMiddleware**: Applies CSP, HSTS, X-Frame-Options
- **AntiforgeryTokenMiddleware**: CSRF protection
- **YARP Reverse Proxy**: Frontend/API gateway functionality

### Authentication Services (AuthenticatedOnly + PermissionBased)

- **Cookie Authentication**: Secure session management
- **OIDC Integration**: Keycloak/OAuth2 authentication
- **Token Refresh**: Automatic token renewal
- **Claims Enrichment**: User information processing

### Authorization Services (PermissionBased Only)

- **PermissionAuthorizationPolicyProvider**: Dynamic policy creation
- **PermissionAuthorizationHandler**: Permission validation
- **RPT Token Service**: Resource Permission Token handling
- **Keycloak Integration**: Permission claim processing

## Examples

### Development Mode (No Authentication)
```json
{
  "affolterNET.Web": {
    "Bff": {
      "Options": {
        "AuthMode": "None",
        "EnableHttpsRedirection": false
      }
    }
  }
}
```

### Internal Tools (Simple Authentication)
```json
{
  "affolterNET.Web": {
    "Bff": {
      "Options": {
        "AuthMode": "Authenticate",
        "EnableSessionManagement": true,
        "EnableTokenRefresh": true
      }
    }
  }
}
```

### Enterprise Applications (Full Authorization)
```json
{
  "affolterNET.Web": {
    "Bff": {
      "Options": {
        "AuthMode": "Authorize",
        "EnableSessionManagement": true,
        "EnableTokenRefresh": true,
        "EnableRptTokens": true,
        "RevokeRefreshTokenOnLogout": true
      }
    }
  }
}
```