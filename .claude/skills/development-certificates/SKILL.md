# Development Certificates Skill

Guidelines for creating and configuring SSL/TLS certificates for local development.

## Tools

### mkcert (Recommended)

mkcert creates locally-trusted development certificates that browsers accept without warnings.

```bash
# Install (macOS)
brew install mkcert

# Install CA into system trust stores (one-time)
mkcert -install

# Find CA root location
mkcert -CAROOT
# Output: /Users/<user>/Library/Application Support/mkcert

# Generate certificate for multiple hostnames
mkcert localhost 127.0.0.1 ::1 <docker-hostnames>
# Creates: server.crt.pem and server.key.pem
```

### OpenSSL (Self-signed fallback)

```bash
# Generate self-signed certificate (browsers will show warnings)
openssl req -x509 -newkey rsa:4096 -keyout server.key.pem -out server.crt.pem \
  -days 365 -nodes -subj "/CN=localhost"
```

## ASP.NET Core Kestrel Configuration

Kestrel requires PFX format certificates.

### Converting PEM to PFX

```bash
# For .NET 9+ (use modern algorithms)
openssl pkcs12 -export -out aspnetapp.pfx \
  -inkey server.key.pem -in server.crt.pem \
  -passout pass:password \
  -certpbe AES-256-CBC -keypbe AES-256-CBC -macalg SHA256

# If the above fails, try legacy mode (OpenSSL 3.0+)
openssl pkcs12 -export -out aspnetapp.pfx \
  -inkey server.key.pem -in server.crt.pem \
  -passout pass:password -legacy
```

### Environment Variables

```yaml
# docker-compose.yml
environment:
  - ASPNETCORE_URLS=https://+:443;http://+:80
  - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
  - ASPNETCORE_Kestrel__Certificates__Default__Password=password
volumes:
  - ./certs:/https:ro
```

### appsettings.json Alternative

```json
{
  "Kestrel": {
    "Certificates": {
      "Default": {
        "Path": "/https/aspnetapp.pfx",
        "Password": "password"
      }
    }
  }
}
```

## Keycloak Configuration

Keycloak uses PEM files directly:

```yaml
# docker-compose.yml
environment:
  - KC_HTTPS_CERTIFICATE_FILE=/opt/keycloak/conf/server.crt.pem
  - KC_HTTPS_CERTIFICATE_KEY_FILE=/opt/keycloak/conf/server.key.pem
command: start-dev --https-port=8443
volumes:
  - ./certs:/opt/keycloak/conf:ro
```

## Browser Trust Issues

### Firefox/Zen Browser

Firefox uses its own certificate store. Even with mkcert CA installed in system:

1. Go to Settings → Privacy & Security → Certificates → View Certificates
2. Authorities tab → Import
3. Select `$(mkcert -CAROOT)/rootCA.pem`
4. Check "Trust this CA to identify websites"
5. Restart browser

### Verifying Certificates

```bash
# Check which certificate a server is using
echo | openssl s_client -connect localhost:5004 -servername localhost 2>/dev/null \
  | openssl x509 -noout -issuer -subject

# Verify PFX file contents
openssl pkcs12 -in aspnetapp.pfx -info -nodes -passin pass:password
```

## Docker Considerations

### Certificate Updates

When updating certificates in mounted volumes:
- `docker-compose restart` may not pick up changes
- Use `docker-compose stop && docker-compose up -d` instead

### Multi-Host Certificates

Generate certificates for all Docker service hostnames:

```bash
mkcert localhost 127.0.0.1 ::1 keycloak example-api example-bff
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| `MOZILLA_PKIX_ERROR_SELF_SIGNED_CERT` | Use mkcert or import CA into browser |
| `CryptographicException` in .NET | Recreate PFX with correct algorithms |
| Certificate not updating in Docker | Stop/start containers, don't just restart |
| `issuer=CN=localhost` (self-signed) | Server using wrong certificate file |
| Firefox still rejects after CA import | Restart Firefox completely |

## File Structure

```
certs/
├── server.crt.pem      # Certificate (PEM)
├── server.key.pem      # Private key (PEM)
└── aspnetapp.pfx       # Combined for Kestrel (PKCS#12)
```
