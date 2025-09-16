# SecurityHeadersOptions Configuration Examples

The SecurityHeadersOptions class provides comprehensive security header configuration with intelligent defaults based on your environment (development vs production).

## Default Behavior

The SecurityHeadersOptions constructor automatically sets different defaults based on the `IsDev` flag from AppSettings. These defaults provide secure settings for production while being more permissive for local development with proxies.

### Development Mode (IsDev = true)
- **HSTS**: Disabled for easier local development (HstsPreload = false)
- **Cross-Origin-Resource-Policy**: "cross-origin" (more relaxed for proxy scenarios)
- **Cross-Origin-Embedder-Policy**: Empty (disabled to avoid local development issues)
- **Connect Sources**: Automatically includes localhost variants for WebSocket and HTTP connections
- **Script Sources**: Includes 'unsafe-eval' for Vue/Vite hot reload support
- **Style Sources**: Includes 'unsafe-inline' for Vue/Vite inline styles support

### Production Mode (IsDev = false)
- **HSTS**: Enabled with 1-year max-age (HstsPreload = true)
- **Cross-Origin-Resource-Policy**: "same-origin" (strict security)
- **Cross-Origin-Embedder-Policy**: "require-corp" (enables SharedArrayBuffer)
- **Connect Sources**: Only explicit allowlisted sources

## Configuration Examples

### Basic Development Configuration
```json
{
  "affolterNET.Web": {
    "SecurityHeaders": {
      "Enabled": true,
      "IdpHost": "https://keycloak.local:8080",
      "AllowedConnectSources": [
        "https://api.local:5001"
      ]
    }
  }
}
```

### Development with Vue/Vite Dev Server
```json
{
  "affolterNET.Web": {
    "SecurityHeaders": {
      "Enabled": true,
      "VueDevServerUrl": "http://localhost:5173",
      "IdpHost": "https://keycloak.local:8080",
      "AllowedConnectSources": [
        "https://api.local:5001"
      ]
    }
  }
}
```

### Production Configuration
```json
{
  "affolterNET.Web": {
    "SecurityHeaders": {
      "Enabled": true,
      "IdpHost": "https://auth.yourdomain.com",
      "AllowedConnectSources": [
        "https://api.yourdomain.com"
      ],
      "AllowedScriptSources": [
        "https://cdn.yourdomain.com"
      ]
    }
  }
}
```

### Custom Security Headers Override
```json
{
  "affolterNET.Web": {
    "SecurityHeaders": {
      "Enabled": true,
      "XFrameOptions": "SAMEORIGIN",
      "ReferrerPolicy": "no-referrer-when-downgrade",
      "PermissionsPolicy": "camera=(), microphone=(), geolocation=()",
      "HstsPreload": false,
      "CrossOriginResourcePolicy": "cross-origin",
      "CustomCspDirectives": {
        "media-src": "'self' https://media.yourdomain.com",
        "worker-src": "'self'"
      }
    }
  }
}
```

### Disable Specific Headers
```json
{
  "affolterNET.Web": {
    "SecurityHeaders": {
      "Enabled": true,
      "XFrameOptions": "",
      "CrossOriginEmbedderPolicy": "",
      "PermissionsPolicy": ""
    }
  }
}
```

## Security Header Properties

| Property | Default (Dev) | Default (Prod) | Description |
|----------|---------------|----------------|-------------|
| `XFrameOptions` | "DENY" | "DENY" | Prevents clickjacking attacks |
| `XContentTypeOptions` | "nosniff" | "nosniff" | Prevents MIME type sniffing |
| `ReferrerPolicy` | "strict-origin-when-cross-origin" | "strict-origin-when-cross-origin" | Controls referrer information |
| `CrossOriginOpenerPolicy` | "same-origin" | "same-origin" | Isolates browsing context |
| `CrossOriginResourcePolicy` | "cross-origin" | "same-origin" | Controls resource sharing |
| `CrossOriginEmbedderPolicy` | "" (disabled) | "require-corp" | Enables SharedArrayBuffer |
| `PermissionsPolicy` | Restrictive | Restrictive | Disables browser features |
| `EnableHsts` | false | true | HTTPS enforcement |
| `HstsPreload` | false | true | HSTS preload directive |

## Tips for Local Development with Proxy

When using a reverse proxy in development (like Vite dev server), you may need to:

1. Set `VueDevServerUrl` to your dev server URL (e.g., "http://localhost:5173")
2. The system automatically includes `'unsafe-inline'` for styles and `'unsafe-eval'` for scripts in development
3. WebSocket connections for hot module reload are automatically allowed
4. Add your backend API servers to `AllowedConnectSources`

These are automatically configured when `IsDev = true`, and the `VueDevServerUrl` property provides convenient configuration for modern frontend frameworks.