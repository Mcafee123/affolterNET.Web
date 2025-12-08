## Tech stack

This project is a collection of .NET libraries for authentication and authorization in ASP.NET Core applications.

### Framework & Runtime
- **Application Framework:** ASP.NET Core (for BFF scenarios), .NET Class Libraries (for reusable components)
- **Language/Runtime:** C# 12 / .NET 9.0
- **Package Manager:** NuGet
- **Build System:** .NET SDK, GitHub Actions

### Backend Architecture
- **Pattern:** Backend-for-Frontend (BFF) with YARP reverse proxy
- **Authentication:** OpenID Connect (OIDC), Cookie Authentication, JWT Bearer tokens
- **Authorization:** Keycloak integration, Permission-based policies, Resource Permission Tokens (RPT)
- **Middleware:** Custom middleware for security headers, CSRF protection, token refresh
- **Caching:** Microsoft.Extensions.Caching (in-memory)

### Key Libraries & Dependencies
- **YARP (Yet Another Reverse Proxy):** v2.2.0 - Reverse proxy for BFF pattern
- **Keycloak.NETCore.Client:** v1.0.2 - Keycloak integration
- **Microsoft.AspNetCore.Authentication.OpenIdConnect:** v9.0.9 - OIDC authentication
- **Microsoft.AspNetCore.Authentication.JwtBearer:** v9.0.9 - JWT authentication for APIs
- **System.IdentityModel.Tokens.Jwt:** v8.14.0 - JWT token handling
- **Swashbuckle.AspNetCore:** v9.0.4 - OpenAPI/Swagger documentation

### Security
- **Security Headers:** CSP, HSTS, X-Frame-Options, X-Content-Type-Options, Referrer-Policy
- **CSRF Protection:** Antiforgery tokens
- **Authentication Modes:** None, Authenticate, Authorize (progressive enhancement)
- **Token Management:** Automatic token refresh, secure cookie storage

### Testing & Quality
- **Test Framework:** xUnit (standard for .NET)
- **Code Style:** EditorConfig, nullable reference types enabled
- **Language Features:** Latest C# (primary constructors, file-scoped namespaces, implicit usings)

### Deployment & Infrastructure
- **Package Distribution:** NuGet.org
- **CI/CD:** GitHub Actions (build, test, pack, publish)
- **Versioning:** Semantic versioning in .csproj files
- **Hosting:** Designed for deployment to any ASP.NET Core hosting environment
