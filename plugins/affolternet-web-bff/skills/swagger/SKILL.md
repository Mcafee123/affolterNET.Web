---
name: swagger
description: Configure Swagger/OpenAPI documentation for affolterNET.Web.Bff. Use when setting up API documentation or customizing Swagger UI.
---

# Swagger/OpenAPI Configuration

Configure Swagger/OpenAPI documentation for your BFF.

For complete reference, see [Library Guide](../../LIBRARY_GUIDE.md).

## Quick Start

### appsettings.json

```json
{
  "affolterNET": {
    "Web": {
      "Swagger": {
        "EnableSwagger": true,
        "RequireAuthentication": true,
        "Title": "My BFF API",
        "Version": "v1"
      }
    }
  }
}
```

## Configuration Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `EnableSwagger` | bool | `true` in dev, `false` otherwise | Enable Swagger UI |
| `RequireAuthentication` | bool | `true` | Gate /swagger behind a signed-in user (challenge redirects to the login; no effect with `AuthenticationMode.None`) |
| `Title` | string | assembly name + env | API title |
| `Version` | string | `"v1"` | API version |
| `ShowHealthEndpoints` | bool | `true` | Include health endpoints in the document |

## BFF-Specific Endpoints

The BFF exposes these endpoints in Swagger:

| Endpoint | Description |
|----------|-------------|
| `/bff/account/login` | Initiates login |
| `/bff/account/logout` | Logs out user |
| `/bff/account/user` | Current user info |
| `/api/*` | Proxied API routes |

## Controller Documentation

```csharp
/// <summary>
/// User profile operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    /// <summary>
    /// Gets the current user's profile
    /// </summary>
    /// <returns>User profile data</returns>
    /// <response code="200">Profile retrieved</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(UserProfile), 200)]
    [ProducesResponseType(401)]
    public IActionResult GetProfile() { ... }
}
```

## Development vs Production

```json
{
  "affolterNET": {
    "Web": {
      "Swagger": {
        "Enabled": true  // Set false in production
      }
    }
  }
}
```

Or use environment-specific configuration:

```json
// appsettings.Development.json
{
  "affolterNET": {
    "Web": {
      "Swagger": {
        "Enabled": true
      }
    }
  }
}

// appsettings.Production.json
{
  "affolterNET": {
    "Web": {
      "Swagger": {
        "Enabled": false
      }
    }
  }
}
```

## Troubleshooting

### Swagger UI shows 401
- Swagger is served before authentication
- Check if path is correctly excluded from auth

### YARP routes not visible
- YARP routes are not documented in Swagger
- Document backend API separately
