# Development Workflow Skill

This skill documents the local development setup for the BFF with Vite HMR (Hot Module Replacement).

## Overview

Two modes of operation:

| Mode | Frontend | When to Use |
|------|----------|-------------|
| **Development** | BFF proxies to Vite dev server | Local development with HMR |
| **Production** | BFF serves static files from `wwwroot/` | Docker, deployed environments |

## Development Mode (Vite HMR)

Run both BFF and Vite dev server:

```bash
# Terminal 1: Start Vite dev server
cd examples/ExampleBff/ClientApp
npm run dev

# Terminal 2: Start BFF in development mode
cd examples/ExampleBff
dotnet run --environment Development
```

Access the app at `https://localhost:5004` - the BFF proxies frontend requests to Vite.

### How It Works

1. BFF runs at `https://localhost:5004`
2. Vite dev server runs at `http://localhost:5173`
3. YARP routes in `appsettings.Development.json` proxy Vite paths to the dev server
4. Vite HMR WebSocket connects through the BFF proxy

### YARP Proxy Routes (Development Only)

```json
{
  "affolterNET": {
    "ReverseProxy": {
      "Routes": {
        "vite-hmr-websocket": { "Match": { "Path": "/", "QueryParameters": [{"Name": "token", "Mode": "Exists"}] } },
        "vite-assets": { "Match": { "Path": "/assets/{**catch-all}" } },
        "vite-client": { "Match": { "Path": "/@vite/{**catch-all}" } },
        "vite-fs": { "Match": { "Path": "/@fs/{**catch-all}" } },
        "vite-src": { "Match": { "Path": "/src/{**catch-all}" } }
      },
      "Clusters": {
        "frontend-cluster": {
          "Destinations": { "frontend": { "Address": "http://localhost:5173" } }
        }
      }
    }
  }
}
```

### Vite HMR Configuration

In `vite.config.ts`:

```typescript
server: {
  port: 5173,
  hmr: {
    host: 'localhost',
    clientPort: 5004,  // Browser connects to BFF
    protocol: 'wss'    // BFF uses HTTPS
  }
}
```

## Production Mode (Docker)

Build frontend and run from static files:

```bash
# Build frontend to wwwroot/
cd examples/ExampleBff/ClientApp
npm run build

# Run BFF (serves static files)
cd examples/ExampleBff
dotnet run --environment Production
```

Or use Docker:

```bash
cd examples
docker compose up
```

## Key Configuration Differences

| Setting | Development | Production |
|---------|-------------|------------|
| `BffOptions.FrontendUrl` | `http://localhost:5173` | Not set |
| `SecurityHeaders.Enabled` | `false` | `true` |
| YARP frontend routes | Configured | Not present |
| Static files | Not used | Served from `wwwroot/` |

## Troubleshooting

### HMR not working
- Verify Vite dev server is running on port 5173
- Check `clientPort: 5004` in vite.config.ts matches BFF port
- Verify `protocol: 'wss'` since BFF uses HTTPS

### CSP errors in development
- CSP is intentionally disabled in development (`SecurityHeaders.Enabled: false`)
- Vite injects inline styles that would violate strict CSP

### Static files not found (production)
- Ensure frontend is built: `npm run build` in ClientApp
- Check `wwwroot/` contains built assets

## Related Files

- `examples/ExampleBff/appsettings.Development.json` - Development YARP routes
- `examples/ExampleBff/appsettings.json` - Production config
- `examples/ExampleBff/ClientApp/vite.config.ts` - Vite HMR configuration
- `examples/docker-compose.yml` - Docker production setup
