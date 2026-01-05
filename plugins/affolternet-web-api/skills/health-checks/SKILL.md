---
name: health-checks
description: Configure health check endpoints for affolterNET.Web.Api. Use when setting up /health endpoints, Kubernetes probes, or monitoring integration.
---

# Health Check Endpoints

Configure health check endpoints for monitoring and container orchestration.

For complete reference, see [Library Guide](../../LIBRARY_GUIDE.md).

## Built-in Endpoints

The API automatically provides these health check endpoints:

| Endpoint | Description | Use Case |
|----------|-------------|----------|
| `/health` | All health checks | General health status |
| `/health/startup` | Startup checks only | Kubernetes startupProbe |
| `/health/ready` | Readiness checks | Kubernetes readinessProbe |

## Built-in Health Checks

| Check | Description |
|-------|-------------|
| `StartupHealthCheck` | Verifies application has started successfully |
| `KeycloakHealthCheck` | Checks Keycloak availability (if auth configured) |
| Self health check | Basic liveness check |

## Kubernetes Integration

### Deployment YAML

```yaml
apiVersion: apps/v1
kind: Deployment
spec:
  template:
    spec:
      containers:
        - name: api
          livenessProbe:
            httpGet:
              path: /health
              port: 80
            initialDelaySeconds: 10
            periodSeconds: 30
          readinessProbe:
            httpGet:
              path: /health/ready
              port: 80
            initialDelaySeconds: 5
            periodSeconds: 10
          startupProbe:
            httpGet:
              path: /health/startup
              port: 80
            failureThreshold: 30
            periodSeconds: 10
```

## Response Format

Health check responses use the standard ASP.NET Core format:

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0234567",
  "entries": {
    "startup": {
      "status": "Healthy",
      "duration": "00:00:00.0001234"
    },
    "keycloak": {
      "status": "Healthy",
      "duration": "00:00:00.0123456"
    }
  }
}
```

## Status Values

| Status | HTTP Code | Description |
|--------|-----------|-------------|
| `Healthy` | 200 | All checks passed |
| `Degraded` | 200 | Some checks degraded but functional |
| `Unhealthy` | 503 | One or more checks failed |

## Common Patterns

### Docker Compose

```yaml
services:
  api:
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

### Load Balancer

Use `/health/ready` for load balancer health checks to ensure the service is ready to receive traffic.

## Troubleshooting

### Health check always unhealthy
- Check logs for specific health check failures
- Verify Keycloak is accessible if auth is configured
- Ensure startup has completed before checks run

### Kubernetes pod keeps restarting
- Increase `initialDelaySeconds` on probes
- Check `startupProbe` threshold is high enough
- Review application startup time
