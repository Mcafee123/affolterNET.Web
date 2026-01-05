# affolterNET.Web Examples

This directory contains example implementations demonstrating the affolterNET.Web authentication library:

- **ExampleApi**: Standalone API using JWT Bearer authentication
- **ExampleBff**: Backend-for-Frontend with Vue.js 3 SPA using cookie-based OIDC + YARP reverse proxy

Both examples demonstrate **Authenticate** and **Authorize** authentication modes with Keycloak.

## Prerequisites

- [Docker](https://www.docker.com/) and Docker Compose
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) (for local development)
- [Node.js 18+](https://nodejs.org/) (for Vue SPA development)

## Quick Start with Docker

```bash
# Navigate to examples directory
cd examples

# Copy environment file
cp .env.example .env

# Start all services (Keycloak, API, BFF)
docker-compose up -d

# Watch startup (optional)
docker-compose logs -f
```

> **Note**: The `keycloak-init` container automatically fixes Keycloak authorization permissions after realm import. The API and BFF services wait for this to complete before starting.

### Access Points

| Service | URL | Description |
|---------|-----|-------------|
| Keycloak Admin | https://localhost:8443 | Admin console (admin/admin) |
| BFF Application | https://localhost:5004 | Vue SPA with authentication |
| API Swagger | https://localhost:5003/swagger | API documentation |

> **Note**: Self-signed certificates are used. Accept the browser security warning to proceed.

## Local Development

For development with hot reload:

### 1. Start Keycloak Only

```bash
docker-compose up -d keycloak
```

### 2. Start API (Terminal 1)

```bash
cd ExampleApi
dotnet run --launch-profile https
```

API will be available at https://localhost:7002

### 3. Start BFF Backend (Terminal 2)

```bash
cd ExampleBff
dotnet run --launch-profile https
```

BFF will be available at https://localhost:7001

### 4. Start Vue Dev Server (Terminal 3)

```bash
cd ExampleBff/ClientApp
npm install
npm run dev
```

Vue SPA will be available at https://localhost:5173 with hot reload (proxies to BFF).

## Test Users

| User | Password | Roles | Permissions |
|------|----------|-------|-------------|
| admin@example.com | admin123 | admin, user, viewer | All resources, all actions |
| user@example.com | user123 | user, viewer | user-resource (read, create, update) |
| viewer@example.com | viewer123 | viewer | user-resource (read only) |

## Authentication Modes

The examples support two authentication modes controlled by the `AuthMode` setting:

### Authenticate Mode

```json
{
  "AuthMode": "Authenticate"
}
```

- Endpoints with `[Authorize]` require a valid login
- No permission checks performed
- Simpler setup, suitable for basic authentication needs

### Authorize Mode

```json
{
  "AuthMode": "Authorize"
}
```

- Full permission-based authorization
- Uses Keycloak RPT (Resource Party Token) tokens
- Endpoints with `[RequirePermission("resource:action")]` validate specific permissions
- Requires Keycloak authorization services configuration

## API Endpoints

### Public Endpoints (No Auth)

| Method | Path | Description |
|--------|------|-------------|
| GET | /api/public | Public information |
| GET | /api/public/health | Health check |

### Protected Endpoints (Authenticate Mode)

| Method | Path | Description |
|--------|------|-------------|
| GET | /api/protected | Returns authenticated user info |
| GET | /api/protected/profile | Returns user profile |

### Permission Endpoints (Authorize Mode)

| Method | Path | Required Permission |
|--------|------|---------------------|
| GET | /api/permission/admin | admin-resource:view |
| POST | /api/permission/admin | admin-resource:manage |
| GET | /api/permission/user | user-resource:read |
| POST | /api/permission/user | user-resource:create |
| PUT | /api/permission/user/{id} | user-resource:update |
| DELETE | /api/permission/user/{id} | user-resource:delete |

## Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Vue.js SPA    │────▶│   BFF (.NET)    │────▶│   API (.NET)    │
│  (Browser)      │     │  Cookie Auth    │     │  JWT Bearer     │
│                 │     │  YARP Proxy     │     │                 │
└─────────────────┘     └─────────────────┘     └─────────────────┘
         │                      │                       │
         │                      │                       │
         └──────────────────────┼───────────────────────┘
                                │
                        ┌───────▼───────┐
                        │   Keycloak    │
                        │   (OIDC/RPT)  │
                        └───────────────┘
```

### Data Flow

1. **User visits SPA** → Vue app loads from BFF
2. **User clicks Login** → Redirected to Keycloak via OIDC
3. **User authenticates** → Keycloak returns to BFF with auth code
4. **BFF stores session** → Cookie-based session established
5. **SPA calls /api/*** → BFF proxies request to API via YARP
6. **YARP adds token** → Access token attached to proxied request
7. **API validates** → JWT Bearer validation + permission check
8. **Response returned** → Through BFF back to SPA

## Configuration

### BFF Configuration (appsettings.json)

```json
{
  "AuthMode": "Authorize",
  "affolterNET": {
    "Web": {
      "Auth": {
        "Provider": {
          "AuthorityBase": "http://localhost:8080",
          "Realm": "demo",
          "ClientId": "bff-client",
          "ClientSecret": "bff-client-secret"
        }
      },
      "BffOptions": {
        "EnableTokenRefresh": true,
        "EnableRptTokens": true,
        "EnableAntiforgery": true
      }
    },
    "ReverseProxy": {
      "Routes": {
        "api-route": {
          "ClusterId": "api-cluster",
          "Match": { "Path": "/api/{**catch-all}" }
        }
      },
      "Clusters": {
        "api-cluster": {
          "Destinations": {
            "api": { "Address": "http://localhost:5002" }
          }
        }
      }
    }
  }
}
```

### API Configuration (appsettings.json)

```json
{
  "AuthMode": "Authorize",
  "affolterNET": {
    "Web": {
      "Auth": {
        "Provider": {
          "AuthorityBase": "http://localhost:8080",
          "Realm": "demo",
          "ClientId": "api-client"
        }
      },
      "Api": {
        "JwtBearer": {
          "ValidateIssuer": true,
          "ValidateAudience": false,
          "RequireHttpsMetadata": false
        }
      }
    }
  }
}
```

## Troubleshooting

### Keycloak not starting

```bash
# Check Keycloak logs
docker-compose logs keycloak

# Restart Keycloak
docker-compose restart keycloak
```

### Login redirects fail

- Ensure Keycloak is running and healthy
- Check redirect URIs in Keycloak client configuration
- Verify `AuthorityBase` matches Keycloak URL

### API returns 401

- Verify the user is logged in
- Check if token has expired (try logging out and back in)
- Verify CORS configuration allows the request origin

### API returns 403

- User is authenticated but lacks the required permission
- Check user's roles in Keycloak
- Verify authorization policies in Keycloak client

### Vue dev server proxy issues

- Ensure BFF backend is running on port 5001
- Check `vite.config.ts` proxy configuration
- Verify no port conflicts

## Customization

### Adding New Resources/Permissions

1. Add resource in Keycloak Admin Console → Clients → bff-client → Authorization → Resources
2. Create policies linking roles to permissions
3. Add permission in API using `[RequirePermission("resource:action")]`
4. Update Vue SPA to check permissions via `authStore.hasPermission()`

### Changing Authentication Mode

1. Update `AuthMode` in both `ExampleApi/appsettings.json` and `ExampleBff/appsettings.json`
2. Restart both applications

## License

This example code is provided as-is for demonstration purposes.
