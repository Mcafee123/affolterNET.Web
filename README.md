# affolterNET.Auth - Authorization Modes

This library provides flexible authentication and authorization modes for ASP.NET Core applications with YARP reverse proxy integration.

## Authorization Modes

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                               AUTHORIZATION MODES                                │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────┐  ┌─────────────────┐  ┌─────────────────────────────────────┐
│      NONE       │  │ AUTHENTICATED   │  │        PERMISSION BASED            │
│                 │  │     ONLY        │  │                                     │
│ Anonymous       │  │ Login Required  │  │ Login + Permission Claims Required  │
│ Access          │  │ No Permissions  │  │ Fine-grained Access Control         │
└─────────────────┘  └─────────────────┘  └─────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                            ALWAYS ENABLED SERVICES                             │
│                        (Security & Infrastructure)                             │
├─────────────────────────────────────────────────────────────────────────────────┤
│ ✅ SecurityHeadersMiddleware     │ CSP, HSTS, X-Frame-Options, etc.            │
│ ✅ AntiforgeryTokenMiddleware    │ CSRF protection                              │
│ ✅ HTTP Context Accessor         │ Core infrastructure                          │
│ ✅ Memory Cache                  │ Performance & caching                        │
│ ✅ YARP Reverse Proxy           │ Frontend/API proxying                        │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                           MODE-SPECIFIC SERVICES                               │
└─────────────────────────────────────────────────────────────────────────────────┘

MODE: NONE                    MODE: AUTHENTICATED ONLY      MODE: PERMISSION BASED
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

┌─────────────────────────────────────────────────────────────────────────────────┐
│                               SERVICE MATRIX                                    │
├─────────────────────────────────┬───────┬───────────────┬─────────────────────┤
│ Service/Middleware              │ NONE  │ AUTHENTICATED │ PERMISSION BASED    │
├─────────────────────────────────┼───────┼───────────────┼─────────────────────┤
│ SecurityHeadersMiddleware       │   ✅   │      ✅        │         ✅           │
│ AntiforgeryTokenMiddleware      │   ✅   │      ✅        │         ✅           │
│ HTTP Context Accessor           │   ✅   │      ✅        │         ✅           │
│ Memory Cache                    │   ✅   │      ✅        │         ✅           │
│ YARP Reverse Proxy             │   ✅   │      ✅        │         ✅           │
├─────────────────────────────────┼───────┼───────────────┼─────────────────────┤
│ Cookie Authentication           │   ❌   │      ✅        │         ✅           │
│ OIDC Integration               │   ❌   │      ✅        │         ✅           │
│ UseAuthentication()            │   ❌   │      ✅        │         ✅           │
│ UseAuthorization()             │   ❌   │      ✅        │         ✅           │
│ RefreshTokenMiddleware         │   ❌   │      ✅        │         ✅           │
│ RptMiddleware                  │   ❌   │      ✅        │         ✅           │
├─────────────────────────────────┼───────┼───────────────┼─────────────────────┤
│ PermissionAuthPolicyProvider    │   ❌   │      ❌        │         ✅           │
│ PermissionAuthHandler          │   ❌   │      ❌        │         ✅           │
│ Keycloak Client                │   ❌   │      ❌        │         ✅           │
│ RPT Token Service              │   ❌   │      ❌        │         ✅           │
│ Permission Service             │   ❌   │      ❌        │         ✅           │
│ Auth Claims Service            │   ❌   │      ❌        │         ✅           │
└─────────────────────────────────┴───────┴───────────────┴─────────────────────┘
```

## Configuration

Configure the authorization mode in your `appsettings.json`:

```json
{
  "Auth": {
    "AuthorizationMode": "AuthenticatedOnly",
    "RequireHttpsMetadata": true,
    "RedirectUri": "/signin-oidc",
    "PostLogoutRedirectUri": "/",
    "Cookie": { 
      "Secure": true 
    }
  }
}
```

### Available Authorization Modes

- **`None`**: Anonymous access, no authentication required
- **`AuthenticatedOnly`**: Login required, no permission checks
- **`PermissionBased`**: Login + fine-grained permission validation

## Usage

### 1. Register Services

```csharp
builder.Services.AddBffAuthentication(builder.Configuration);
```

### 2. Configure Middleware Pipeline

```csharp
app.UseCompleteBffAuthentication();
```

## Key Features

- **Progressive Enhancement**: Each mode builds upon the previous one
- **YARP Integration**: Reverse proxy works seamlessly in all modes
- **Security First**: CSP, Antiforgery, and Security Headers always enabled
- **Flexible Configuration**: Easy mode switching via configuration
- **Clean Service Registration**: Only required services are registered per mode

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
{ "Auth": { "AuthorizationMode": "None" } }
```

### Internal Tools (Simple Authentication)
```json
{ "Auth": { "AuthorizationMode": "AuthenticatedOnly" } }
```

### Enterprise Applications (Full Authorization)
```json
{ "Auth": { "AuthorizationMode": "PermissionBased" } }
```