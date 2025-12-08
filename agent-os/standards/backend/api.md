## API endpoint standards and conventions

### ASP.NET Core Controller Patterns (Observed)
- **ApiController Attribute**: All API controllers use `[ApiController]` for automatic model validation and binding
- **ControllerBase**: Inherit from `ControllerBase` (not `Controller`) for API-only controllers
- **Route Templates**: Use `[Route("bff/[controller]")]` or explicit routes like `[Route("bff/account")]`
- **Primary Constructors**: Use primary constructors for dependency injection (e.g., `public class UserController(IClaimsEnrichmentService service)`)

### HTTP Methods & Actions
- **HTTP Verb Attributes**: Use `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]` on action methods
- **Action Naming**: Use descriptive names (e.g., `GetCurrentUser`, `GetUserPermissions`, `GetCurrentUserDetailed`)
- **Async Actions**: All I/O operations are async with `Task<IActionResult>` return type
- **CancellationToken**: Include `CancellationToken cancellationToken` parameter in async actions

### Route Structure (BFF Pattern)
- **Prefix Pattern**: Use `/bff/` prefix for BFF endpoints (e.g., `/bff/user`, `/bff/account`)
- **Resource-Based**: Routes follow resource naming (e.g., `/bff/user`, `/bff/user/me`, `/bff/user/permissions`)
- **No Versioning**: Not used in this project; would use `/api/v1/` pattern if needed

### Authorization & Authentication
- **Authorize Attribute**: Use `[Authorize]` for protected endpoints
- **AllowAnonymous**: Use `[AllowAnonymous]` for public endpoints
- **Permission Attributes**: Use custom `[RequirePermission("permission-name")]` for fine-grained authorization
- **Role Attributes**: Use `[RequireRole("role-name")]` for role-based authorization

### CSRF Protection
- **Antiforgery Tokens**: Enabled by default for state-changing operations (POST, PUT, DELETE)
- **IgnoreAntiforgeryToken**: Use `[IgnoreAntiforgeryToken]` for endpoints that don't need CSRF protection (e.g., read-only GET endpoints)

### Response Patterns (Observed)
- **IActionResult**: Standard return type allowing `Ok()`, `NotFound()`, `BadRequest()`, etc.
- **Ok() with Data**: Return `Ok(new { prop1 = value1, prop2 = value2 })` for successful responses
- **Anonymous Objects**: Use anonymous objects for simple DTOs (e.g., `Ok(new { IsAuthenticated = true, Claims = ... })`)
- **Structured Responses**: Consistent response structure (e.g., UserContext, permissions)

### Status Codes (Standard Usage)
- **200 OK**: Successful GET requests, returns data
- **201 Created**: Successful POST that creates a resource
- **401 Unauthorized**: Not authenticated (BFF pattern returns this instead of redirecting)
- **403 Forbidden**: Authenticated but lacks permission
- **404 Not Found**: Resource not found

### Error Handling (BFF Pattern)
- **No Redirect on 401**: BFF pattern returns 401 status instead of redirecting to IDP
- **SPA Error Handling**: Let SPA handle displaying login UI; API just returns status codes
- **Middleware Handling**: Global error handling via middleware (not per-controller try/catch)

### API Documentation
- **XML Comments**: All actions have `/// <summary>` documentation
- **Swagger/OpenAPI**: Swashbuckle.AspNetCore for API documentation
- **Endpoint Description**: Explain purpose and behavior in XML comments

### Service Injection Patterns
- **Constructor Injection**: Use primary constructors for all dependencies
- **Scoped Services**: Request-scoped services injected into controllers (e.g., `IClaimsEnrichmentService`)
- **Avoid Service Locator**: Never use `HttpContext.RequestServices.GetService<T>()`

### Response Formatting
- **JSON by Default**: ASP.NET Core defaults to JSON serialization
- **Camel Case**: JSON properties use camelCase (ASP.NET Core default)
- **Null Handling**: Nullable properties handled explicitly with C# nullable reference types
