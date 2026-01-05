---
name: security
description: Configure security headers, CORS, and the IConfigurableOptions pattern for affolterNET.Web.Api. Use when setting up CSP, HSTS, CORS policies, or custom options.
---

# Security Configuration

Configure security headers, CORS, and the options pattern.

For complete reference, see [Library Guide](../../LIBRARY_GUIDE.md).

## Security Headers

### appsettings.json

```json
{
  "affolterNET": {
    "Web": {
      "SecurityHeaders": {
        "EnableHsts": true,
        "EnableXFrameOptions": true,
        "EnableXContentTypeOptions": true,
        "EnableReferrerPolicy": true,
        "ContentSecurityPolicy": "default-src 'self'"
      }
    }
  }
}
```

### Program.cs

```csharp
var options = builder.Services.AddApiServices(isDev, config, opts => {
    opts.EnableSecurityHeaders = true;
});
```

## CORS Configuration

### appsettings.json

```json
{
  "affolterNET": {
    "Web": {
      "Cors": {
        "AllowedOrigins": ["https://app.example.com", "https://admin.example.com"],
        "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
        "AllowedHeaders": ["Content-Type", "Authorization"],
        "AllowCredentials": true,
        "MaxAge": 3600
      }
    }
  }
}
```

## IConfigurableOptions Pattern

All options follow a three-tier configuration pattern:

```csharp
// 1. Defaults are set in constructor
// 2. appsettings.json values override defaults
// 3. Lambda configuration overrides appsettings

var options = builder.Services.AddApiServices(isDev, config, opts => {
    // This lambda is tier 3 - highest priority
    opts.ConfigureApi = api => {
        api.AuthMode = AuthenticationMode.Authorize;
    };
});
```

## Configuration Sections

| Section | Options Class |
|---------|---------------|
| `affolterNET:Web:SecurityHeaders` | `SecurityHeadersOptions` |
| `affolterNET:Web:Cors` | `AffolterNetCorsOptions` |
| `affolterNET:Web:Auth:Provider` | `AuthProviderOptions` |

## Common Patterns

### Development-specific CORS

```csharp
// CORS is typically more permissive in development
// The isDev flag passed to AddApiServices handles this
var options = builder.Services.AddApiServices(
    builder.Environment.IsDevelopment(),
    builder.Configuration);
```

### Custom CSP for APIs

```json
{
  "affolterNET": {
    "Web": {
      "SecurityHeaders": {
        "ContentSecurityPolicy": "default-src 'none'; frame-ancestors 'none'"
      }
    }
  }
}
```

## Troubleshooting

### CORS preflight fails
- Ensure `AllowedOrigins` includes the exact origin (including protocol and port)
- Check that `AllowedMethods` includes the HTTP method being used
- Verify `AllowCredentials` is true if sending cookies/auth headers

### CSP blocks resources
- Review browser console for CSP violation reports
- Add required sources to the appropriate CSP directive
- Consider using `report-uri` directive for monitoring
