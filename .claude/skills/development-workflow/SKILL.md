# Development Workflow Skill

This skill documents the local development setup for the BFF with Vite HMR (Hot Module Replacement).

## Overview

Three modes of operation:

| Mode | Infrastructure | Frontend | When to Use |
|------|----------------|----------|-------------|
| **Full Docker** | All in Docker | Built static files | E2E testing, CI/CD |
| **Local Dev** | Keycloak in Docker | BFF + Vite local | Development with HMR |
| **Production** | External | Built static files | Deployed environments |

## Development Mode (Recommended)

### Quick Start - Single Command

Start everything with one command:

```bash
cd examples/ExampleBff/ClientApp
npm run dev:full
```

This will:
1. Start Keycloak + keycloak-init in Docker (background)
2. Wait for Keycloak to be healthy
3. Start BFF and Vite concurrently with colored output

Access the app at `https://localhost:5004`

### Available npm Scripts

| Script | Description |
|--------|-------------|
| `npm run dev:full` | Start everything: Keycloak + BFF + Vite |
| `npm run dev` | Start Vite dev server only |
| `npm run keycloak:start` | Start Keycloak in Docker |
| `npm run keycloak:stop` | Stop Keycloak container |
| `npm run keycloak:logs` | Follow Keycloak logs |
| `npm run keycloak:wait` | Wait for Keycloak health check |
| `npm run bff:start` | Start BFF with HTTPS (port 5004) |
| `npm run build` | Build frontend to `../wwwroot/` |

### Manual Setup (Separate Terminals)

If you prefer running components separately:

```bash
# Terminal 1: Start Keycloak
cd examples/ExampleBff/ClientApp
npm run keycloak:start

# Terminal 2: Start BFF
npm run bff:start

# Terminal 3: Start Vite dev server
npm run dev
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

- `examples/ExampleBff/ClientApp/package.json` - npm scripts for development
- `examples/ExampleBff/appsettings.Development.json` - Development YARP routes
- `examples/ExampleBff/appsettings.json` - Production config
- `examples/ExampleBff/ClientApp/vite.config.ts` - Vite HMR configuration
- `examples/ExampleBff/Properties/launchSettings.json` - BFF launch profiles
- `examples/docker-compose.yml` - Docker setup (full or Keycloak-only)
