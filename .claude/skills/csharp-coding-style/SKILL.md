# C# Coding Style

Use this skill when writing or modifying C# code in this repository.

## Language Features

- **Target**: C# 12 / .NET 9.0
- **File-scoped namespaces**: Always use single-line namespace declaration
- **Primary constructors**: Preferred for dependency injection
- **Nullable reference types**: Enabled throughout

## Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase with descriptive suffix | `PermissionService`, `RefreshTokenMiddleware` |
| Interfaces | `I` prefix + PascalCase | `IPermissionService`, `IClaimsEnrichmentService` |
| Public methods | PascalCase | `GetUserPermissionsAsync()` |
| Async methods | `Async` suffix | `EnrichUserContextAsync()` |
| Private fields | `_camelCase` with underscore | `_rptConfig`, `_logger` |
| Parameters | camelCase | `userId`, `accessToken` |
| Properties | PascalCase | `UserId`, `IsAuthenticated` |
| Constants | PascalCase | `SectionName`, `NonceKey` |

### Class Suffixes

- Options classes: `*Options` (e.g., `BffAppOptions`)
- Services: `*Service` (e.g., `PermissionService`)
- Middleware: `*Middleware` (e.g., `SecurityHeadersMiddleware`)
- Handlers: `*Handler` (e.g., `PermissionAuthorizationHandler`)
- Extensions: `*Extensions` (e.g., `ServiceCollectionExtensions`)

## Formatting

- **Indentation**: 4 spaces (no tabs)
- **Braces**: K&R style (opening brace on same line)
- **Always use braces**: Even for single-line if/foreach statements
- **Line length**: Keep reasonable, break long method chains

```csharp
public class ExampleService(
    ILogger<ExampleService> logger,
    IOptionsMonitor<ExampleOptions> options)
    : IExampleService
{
    private readonly ExampleOptions _config = options.CurrentValue;

    public async Task<Result> ProcessAsync(string input, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(input))
        {
            return Result.Empty;
        }

        var result = await _someService.DoWorkAsync(input, cancellationToken);
        return result;
    }
}
```

## Property Patterns

```csharp
// Auto-properties with initializers
public string Name { get; set; } = string.Empty;
public IReadOnlyList<string> Items { get; set; } = Array.Empty<string>();

// Expression-bodied for computed values
public string FullPath => $"{BasePath}/{Name}";

// Nullable properties
public string? OptionalValue { get; set; }
```

## Constructor Patterns

**Primary constructors** for DI (preferred):
```csharp
public class PermissionService(
    RptTokenService rptTokenService,
    IMemoryCache cache,
    ILogger<PermissionService> logger,
    IOptionsMonitor<PermissionCacheOptions> options)
    : IPermissionService
{
    private readonly PermissionCacheOptions _config = options.CurrentValue;
}
```

**Traditional constructors** for options classes:
```csharp
public AuthProviderOptions() : this(new AppSettings())
{
}

private AuthProviderOptions(AppSettings settings)
{
    AuthorityBase = string.Empty;
}
```

## Async Patterns

- All I/O operations must be async
- Always include `CancellationToken` parameter (optional with default)
- Use `Task.CompletedTask` for no-op async methods

```csharp
public async Task<IReadOnlyList<Permission>> GetPermissionsAsync(
    string userId,
    CancellationToken cancellationToken = default)
{
    if (string.IsNullOrEmpty(userId))
    {
        return Array.Empty<Permission>();
    }

    return await _cache.GetOrCreateAsync(userId, async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        return await _service.FetchPermissionsAsync(userId, cancellationToken);
    }) ?? Array.Empty<Permission>();
}
```

## Null Handling

- Use nullable reference types (`string?`, `Permission?`)
- Initialize collections to empty: `Array.Empty<T>()`, `string.Empty`
- Null-coalescing for defaults: `value ?? string.Empty`
- Safe navigation: `context.User.Identity?.IsAuthenticated`
- Guard clauses at method start

```csharp
public async Task<Result> ProcessAsync(string? input)
{
    if (string.IsNullOrEmpty(input))
    {
        return Result.Empty;
    }

    var value = _cache.Get(input) ?? await FetchAsync(input);
    return value;
}
```

## LINQ Usage

```csharp
// Method chaining with proper line breaks
var roles = principal.FindAll(ClaimTypes.Role)
    .Select(c => c.Value)
    .Concat(principal.FindAll("roles").Select(c => c.Value))
    .Distinct()
    .ToList();

// Dictionary creation
var claims = principal.Claims
    .GroupBy(c => c.Type)
    .ToDictionary(
        g => g.Key,
        g => g.Count() == 1
            ? (object)g.First().Value
            : g.Select(c => c.Value).ToArray());
```

## Documentation

- XML docs on public classes and methods
- Inline comments only for complex logic (explain "why", not "what")

```csharp
/// <summary>
/// Service for managing user permissions via Keycloak RPT tokens.
/// </summary>
public class PermissionService : IPermissionService
{
    /// <summary>
    /// Gets current user permissions from cache or Keycloak.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of user permissions.</returns>
    public async Task<IReadOnlyList<Permission>> GetPermissionsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Check cache first to avoid unnecessary Keycloak calls
        // during high-frequency permission checks
        if (_cache.TryGetValue(userId, out var cached))
        {
            return cached;
        }
        // ...
    }
}
```

## Dependency Injection

```csharp
// Service registration
services.AddScoped<IPermissionService, PermissionService>();
services.AddSingleton<RptCacheService>();
services.AddHttpClient<IBffApiClient, BffApiClient>();

// Use IOptionsMonitor<T> for options that may change
// Use IOptions<T> for static configuration
```

## Method Organization in Classes

1. Constructor(s)
2. Public properties
3. Public methods (interface implementations first)
4. Private/protected methods

## Access Modifier Order

`public/private/protected` → `static` → `readonly` → `async`

```csharp
public static string SectionName => "affolterNET:Web";
private readonly PermissionCacheOptions _config;
public async Task<Result> ProcessAsync(...)
private static void ValidateInput(...)
```
