# Keycloak Configuration Skill

This skill documents how to configure Keycloak for use with the affolterNET.Web authentication library.

## Overview

Keycloak is used as the identity provider (IdP) for both the API (JWT Bearer) and BFF (Cookie OIDC) authentication patterns. This document covers the configuration required for:

- Realm and client setup
- OIDC client scopes and protocol mappers
- Role-based access control (RBAC)
- Authorization services (RPT-based permissions)

## Realm Configuration

### Basic Settings

```json
{
  "realm": "demo",
  "enabled": true,
  "sslRequired": "none",
  "loginWithEmailAllowed": true,
  "accessTokenLifespan": 300,
  "ssoSessionIdleTimeout": 1800,
  "ssoSessionMaxLifespan": 36000
}
```

### Realm Roles

Define realm roles for RBAC:

```json
"roles": {
  "realm": [
    { "name": "admin", "description": "Administrator role with full access" },
    { "name": "user", "description": "Standard user role" },
    { "name": "viewer", "description": "Read-only viewer role" }
  ]
}
```

### User Configuration

Assign realm roles to users:

```json
{
  "username": "admin@example.com",
  "email": "admin@example.com",
  "emailVerified": true,
  "enabled": true,
  "credentials": [{ "type": "password", "value": "admin123", "temporary": false }],
  "realmRoles": ["admin", "user", "viewer"]
}
```

## Client Configuration

### BFF Client (Confidential)

For cookie-based OIDC authentication with the BFF pattern:

```json
{
  "clientId": "bff-client",
  "secret": "bff-client-secret",
  "publicClient": false,
  "protocol": "openid-connect",
  "standardFlowEnabled": true,
  "directAccessGrantsEnabled": true,
  "serviceAccountsEnabled": true,
  "authorizationServicesEnabled": true,
  "redirectUris": ["https://localhost:5004/*"],
  "webOrigins": ["https://localhost:5004"]
}
```

### API Client (Bearer-Only)

For JWT Bearer authentication:

```json
{
  "clientId": "api-client",
  "bearerOnly": true,
  "publicClient": false,
  "protocol": "openid-connect",
  "standardFlowEnabled": false
}
```

## Client Scopes and Protocol Mappers

### Critical: Standard OIDC Scopes

When importing a realm via JSON, you **must** define the standard OIDC client scopes. Keycloak's default scopes are not automatically available during import.

Required client scopes:

1. **openid** - Required for OIDC
2. **profile** - User profile claims (name, username)
3. **email** - Email claims
4. **roles** - Role claims (custom configuration needed)
5. **web-origins** - CORS allowed origins
6. **acr** - Authentication Context Class Reference

### Roles Client Scope Configuration

**Important**: The `roles` scope must be configured to output roles as a **flat claim**, not nested under `realm_access.roles`.

The BFF library's `BffClaimsEnrichmentService` expects roles in a claim named `"roles"`:

```csharp
var roles = principal.FindAll(_claimTypes.Roles)  // defaults to "roles"
    .Select(c => c.Value)
    .ToList();
```

#### Correct Roles Mapper Configuration

```json
{
  "name": "roles",
  "protocol": "openid-connect",
  "attributes": {
    "include.in.token.scope": "false",
    "display.on.consent.screen": "true"
  },
  "protocolMappers": [
    {
      "name": "realm roles",
      "protocol": "openid-connect",
      "protocolMapper": "oidc-usermodel-realm-role-mapper",
      "config": {
        "user.attribute": "foo",
        "access.token.claim": "true",
        "claim.name": "roles",
        "jsonType.label": "String",
        "multivalued": "true",
        "id.token.claim": "true",
        "userinfo.token.claim": "true"
      }
    }
  ]
}
```

**Key Points:**
- `claim.name`: Must be `"roles"` (flat), NOT `"realm_access.roles"` (nested)
- `include.in.token.scope`: Set to `"false"` for default scopes
- `multivalued`: Must be `"true"` for multiple roles

### Default Client Scopes

Assign scopes to clients:

```json
"defaultClientScopes": [
  "openid",
  "web-origins",
  "acr",
  "profile",
  "roles",
  "email"
]
```

**Important**: Do NOT request default client scopes explicitly in the OIDC scope parameter. They are automatically included. Requesting them explicitly causes Keycloak to return "Invalid scopes" error.

#### BFF appsettings.json - Correct

```json
"Oidc": {
  "Scopes": "openid profile email"
}
```

#### BFF appsettings.json - Incorrect (causes error)

```json
"Oidc": {
  "Scopes": "openid profile email roles"  // "roles" causes error if it's a default scope
}
```

## Authorization Services (RPT Tokens)

### Overview

Keycloak Authorization Services provide fine-grained, policy-based access control using:
- **Resources**: Protected items (e.g., "admin-resource", "user-resource")
- **Scopes**: Actions on resources (e.g., "view", "manage", "read", "write")
- **Policies**: Rules that determine access (e.g., role-based policies)
- **Permissions**: Link resources/scopes to policies

### Configuration Structure

```json
"authorizationSettings": {
  "allowRemoteResourceManagement": true,
  "policyEnforcementMode": "ENFORCING",
  "resources": [...],
  "scopes": [...],
  "policies": [...],
  "decisionStrategy": "UNANIMOUS"
}
```

### Resources

```json
"resources": [
  {
    "name": "admin-resource",
    "displayName": "Admin Resource",
    "type": "admin",
    "scopes": [
      { "name": "view" },
      { "name": "manage" }
    ]
  }
]
```

### Scopes (Global)

```json
"scopes": [
  { "name": "view" },
  { "name": "manage" },
  { "name": "read" },
  { "name": "create" },
  { "name": "update" },
  { "name": "delete" }
]
```

### Policies

Role-based policies:

```json
"policies": [
  {
    "name": "Admin Role Policy",
    "type": "role",
    "logic": "POSITIVE",
    "decisionStrategy": "UNANIMOUS",
    "config": {
      "roles": "[{\"id\":\"admin\",\"required\":true}]"
    }
  }
]
```

### Permissions (Scope-Based)

Permissions are defined as policies with `type: "scope"`:

```json
{
  "name": "Admin View Permission",
  "type": "scope",
  "logic": "POSITIVE",
  "decisionStrategy": "UNANIMOUS",
  "config": {
    "resources": "[\"admin-resource\"]",
    "scopes": "[\"view\"]",
    "applyPolicies": "[\"Admin Role Policy\"]"
  }
}
```

### Known Limitation: JSON Import

**Authorization services configured via JSON realm import often don't work correctly.**

The role policies reference roles by name (e.g., `"id": "admin"`), but Keycloak's import process doesn't always properly link the policies to the actual realm roles. This results in:

```
Error fetching RPT: {"error":"access_denied","error_description":"not_authorized"}
```

#### Solution: Automated Permission Fix Script

The repository includes `examples/keycloak/fix-permissions.sh` that automatically recreates scope-based permissions with proper resource/scope/policy links after realm import.

**Docker Compose Integration:**

The `keycloak-init` container runs automatically after Keycloak starts:

```yaml
keycloak-init:
  image: python:3.12-alpine
  depends_on:
    keycloak:
      condition: service_healthy
  environment:
    - KEYCLOAK_URL=https://keycloak:8443
    - KEYCLOAK_ADMIN=${KEYCLOAK_ADMIN:-admin}
    - KEYCLOAK_ADMIN_PASSWORD=${KEYCLOAK_ADMIN_PASSWORD:-admin}
    - SSL_CERT_FILE=/certs/rootCA.pem
  volumes:
    - ./keycloak/fix-permissions.sh:/fix-permissions.sh:ro
    - ./certs:/certs:ro
  entrypoint: ["/bin/sh", "-c", "apk add --no-cache curl bash && bash /fix-permissions.sh"]
```

**Running Manually (local development):**

```bash
cd examples/keycloak
KEYCLOAK_URL=https://localhost:8443 ./fix-permissions.sh
```

The script:
1. Waits for Keycloak health check
2. Gets admin token
3. Deletes broken scope permissions
4. Recreates permissions with proper resource/scope/policy UUID links
5. Verifies RPT token acquisition

#### Fallback: Manual Configuration

If automated fix doesn't work, configure via Keycloak Admin Console:

1. Open Keycloak Admin Console (https://localhost:8443)
2. Select your realm (e.g., "demo")
3. Navigate to: Clients > bff-client > Authorization tab
4. Configure Resources, Scopes, Policies, and Permissions manually
5. Use the "Evaluate" tab to test permissions

## Docker Configuration

### Keycloak Container

```yaml
keycloak:
  image: quay.io/keycloak/keycloak:26.0
  environment:
    - KC_BOOTSTRAP_ADMIN_USERNAME=${KEYCLOAK_ADMIN:-admin}
    - KC_BOOTSTRAP_ADMIN_PASSWORD=${KEYCLOAK_ADMIN_PASSWORD:-admin}
    - KC_HEALTH_ENABLED=true
    - KC_HTTPS_CERTIFICATE_FILE=/opt/keycloak/conf/server.crt.pem
    - KC_HTTPS_CERTIFICATE_KEY_FILE=/opt/keycloak/conf/server.key.pem
    - KC_HOSTNAME=https://localhost:8443
    - KC_HOSTNAME_BACKCHANNEL_DYNAMIC=true
    - KC_HTTP_ENABLED=true
    - KC_LEGACY_OBSERVABILITY_INTERFACE=true
  command: start --import-realm --https-port=8443 --http-port=8080
  volumes:
    - ./keycloak/realm-export.json:/opt/keycloak/data/import/realm-export.json:ro
    - ./certs:/opt/keycloak/conf:ro
  healthcheck:
    test: ["CMD-SHELL", "exec 3<>/dev/tcp/127.0.0.1/8080;echo -e 'GET /health/ready HTTP/1.1\\r\\nhost: localhost\\r\\nConnection: close\\r\\n\\r\\n' >&3;if [ $? -eq 0 ]; then exit 0; else exit 1; fi"]
    interval: 10s
    timeout: 5s
    retries: 10
    start_period: 30s
```

**Configuration Notes:**
- `KC_BOOTSTRAP_ADMIN_USERNAME` / `KC_BOOTSTRAP_ADMIN_PASSWORD` - Admin credentials
- `KC_HOSTNAME` - Full URL including protocol (hostname v2 format)
- `KC_HOSTNAME_BACKCHANNEL_DYNAMIC=true` - Enables dynamic backchannel URLs for Docker internal networking
- `KC_LEGACY_OBSERVABILITY_INTERFACE=true` - Exposes health endpoints on HTTP port
- `start` command runs in production mode with `--import-realm` for initial setup

### HTTPS with Custom CA (mkcert)

For internal Docker communication with HTTPS:

1. Generate certificates with mkcert
2. Mount certificates to containers
3. Install CA in container trust store

#### ASP.NET Application Configuration

```yaml
example-bff:
  environment:
    - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
    - ASPNETCORE_Kestrel__Certificates__Default__Password=password
    - affolterNET__Web__Auth__Provider__AuthorityBase=https://keycloak:8443
    - SSL_CERT_FILE=/https/rootCA.pem
  volumes:
    - ./certs:/https:ro
```

#### docker-entrypoint.sh

```bash
#!/bin/bash
set -e

if [ -f "/https/rootCA.pem" ]; then
    echo "Adding custom CA certificate to trust store..."
    cp /https/rootCA.pem /usr/local/share/ca-certificates/mkcert-ca.crt
    update-ca-certificates
fi

exec dotnet ExampleBff.dll
```

#### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install ca-certificates for custom CA support
RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .
COPY examples/ExampleBff/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

ENTRYPOINT ["/docker-entrypoint.sh"]
```

## Troubleshooting

### "Invalid scopes" Error

**Cause**: Requesting a scope that's already a default client scope.

**Solution**: Remove the scope from the explicit `Scopes` configuration in appsettings.json.

### Roles Not Appearing in Token/Claims

**Cause**: Roles mapper using nested claim name (`realm_access.roles`).

**Solution**: Configure the roles mapper with `claim.name: "roles"` (flat).

### RPT Token "not_authorized" Error

**Cause**: Authorization services not properly configured after JSON import.

**Solution**: Run the `fix-permissions.sh` script to recreate scope permissions with proper links:

```bash
cd examples/keycloak
KEYCLOAK_URL=https://localhost:8443 ./fix-permissions.sh
```

In Docker, the `keycloak-init` container runs this automatically. If the script fails, configure authorization policies manually in Keycloak Admin Console.

### Certificate Trust Issues in Docker

**Cause**: Container doesn't trust the custom CA.

**Solution**:
1. Mount the rootCA.pem to the container
2. Use an entrypoint script to install the CA via `update-ca-certificates`
3. Set `SSL_CERT_FILE` environment variable

## Reference: Example realm-export.json Structure

```
realm-export.json
├── realm settings (name, ssl, timeouts)
├── roles
│   └── realm[] (admin, user, viewer)
├── users[] (with realmRoles assignments)
├── clients[]
│   ├── bff-client (confidential, authorization enabled)
│   │   ├── defaultClientScopes
│   │   └── authorizationSettings
│   │       ├── resources[]
│   │       ├── scopes[]
│   │       ├── policies[] (role policies + permission policies)
│   │       └── decisionStrategy
│   └── api-client (bearer-only)
└── clientScopes[]
    ├── openid
    ├── profile
    ├── email
    ├── roles (with realm roles mapper)
    ├── web-origins
    └── acr
```

## Related Files

- `examples/keycloak/realm-export.json` - Full realm configuration
- `examples/keycloak/fix-permissions.sh` - Script to fix authorization permissions after import
- `examples/docker-compose.yml` - Docker service configuration (includes keycloak-init container)
- `examples/ExampleBff/appsettings.json` - BFF OIDC configuration
- `examples/ExampleApi/appsettings.json` - API JWT Bearer configuration
