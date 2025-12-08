## Error handling best practices

### ASP.NET Core Error Handling (Observed Patterns)
- **Middleware-Based**: Global error handling via middleware at the pipeline level
- **No Try-Catch in Controllers**: Controllers delegate error handling to middleware (clean separation of concerns)
- **Status Code Responses**: Return appropriate HTTP status codes (401, 403, 404, 500) without exposing stack traces

### BFF Pattern Error Handling
- **401 Instead of Redirects**: Return 401 Unauthorized instead of redirecting to IDP; let SPA handle login UI
- **Event Interception**: Use OIDC events (e.g., `OnRedirectToIdentityProvider`) to intercept and customize behavior
- **No Automatic Redirects**: `context.HandleResponse()` prevents automatic redirects; explicitly return status codes

### Validation & Preconditions
- **Fail Fast**: Validate input early; check preconditions before processing
- **Model Validation**: Leverage `[ApiController]` for automatic model validation
- **Explicit Null Checks**: Use nullable reference types and null-conditional operators (`?.`, `??`)

### Resource Management
- **Using Statements**: Use `using` for IDisposable resources (automatically handles cleanup)
- **HttpClient Factory**: Use `IHttpClientFactory` instead of creating HttpClient instances directly
- **Async Disposal**: Use `await using` for async disposable resources

### External Service Calls
- **Retry Logic**: Not observed in this codebase; would implement exponential backoff for production
- **Timeout Handling**: Use `CancellationToken` to support timeouts and cancellation
- **Circuit Breaker**: Not implemented; consider for production external API calls

### Logging (Not extensively used in this library)
- **ILogger Injection**: Inject `ILogger<T>` for structured logging
- **Log Levels**: Use appropriate levels (Error, Warning, Information, Debug)
- **Sensitive Data**: Never log passwords, tokens, or sensitive information

### Security Considerations
- **No Stack Traces to Client**: Never expose stack traces or internal errors to API consumers
- **Generic Error Messages**: Return generic messages (e.g., "An error occurred") for security-sensitive failures
- **Detailed Logs**: Log detailed errors server-side for debugging; sanitize client responses

### Specific Exception Handling
- **Avoid Catching Everything**: Don't use `catch (Exception ex)` unless necessary; catch specific exceptions
- **Re-throw When Needed**: Use `throw;` (not `throw ex;`) to preserve stack trace when re-throwing
- **Custom Exceptions**: Create custom exception types for domain-specific errors (not extensively used in this codebase)
