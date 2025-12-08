## Coding style best practices

### C# Language Features (Based on Project Code)
- **Use C# 12+ Features**: Leverage primary constructors, file-scoped namespaces, latest language features
- **Nullable Reference Types**: Always enabled (`<Nullable>enable</Nullable>`); handle nullability explicitly with `?` and null-coalescing operators
- **Implicit Usings**: Enabled (`<ImplicitUsings>enable</ImplicitUsings>`) for cleaner files
- **File-Scoped Namespaces**: Use file-scoped namespaces (e.g., `namespace MyNamespace;`) instead of block-scoped
- **Primary Constructors**: Use primary constructors for dependency injection (e.g., `public class MyClass(IService service)`)
- **Latest Language Version**: Use `<LangVersion>latest</LangVersion>` in .csproj files

### Naming Conventions (C# Standard - Observed in Codebase)
- **PascalCase**: Classes, methods, properties, namespaces, public fields, enums (e.g., `SecurityHeadersMiddleware`, `AddBffServices`, `AuthenticationMode`)
- **camelCase**: Parameters, local variables, private fields (e.g., `next`, `options`, `_logger`)
- **Interface Prefix `I`**: All interfaces (e.g., `IClaimsEnrichmentService`, `IRptService`, `IConfigurableOptions`)
- **Async Suffix**: Async methods end with `Async` (e.g., `EnrichUserContextAsync`, `InvokeAsync`)
- **Descriptive Names**: Full, descriptive names; abbreviations only for well-known terms (e.g., `Bff`, `Rpt`, `Oidc`, `Csp`)

### Code Organization Patterns (Observed)
- **Extension Methods**: Use for fluent service registration (e.g., `AddBffServices`, `ConfigureBffApp`, `AddCoreServices`)
- **Options Pattern**: Use `IConfigurableOptions<T>` interface with `SectionName` property for configuration binding
- **Dependency Injection**: Constructor injection via primary constructors; appropriate lifetimes (Singleton, Scoped, Transient)
- **Middleware Pattern**: `RequestDelegate next` with `public async Task Invoke(HttpContext context)` or `InvokeAsync`
- **Internal Extension Methods**: Mark as `private static` or `internal` when used only within the assembly

### Documentation (Observed Standards)
- **XML Comments**: All public APIs have `/// <summary>` documentation
- **Parameter Documentation**: Use `/// <param>` for non-obvious parameters
- **Enum Documentation**: Document each enum value with `/// <summary>`
- **Purpose Explanation**: Explain *why* not just *what* (e.g., "// BFF pattern: Prevent automatic redirects to IDP")

### Async/Await Patterns (Consistently Applied)
- **Async All I/O**: All I/O operations use async/await
- **CancellationToken**: Include `CancellationToken cancellationToken` in async methods
- **Async Method Signature**: `public async Task<T>` or `public async Task`
- **ConfigureAwait**: Not observed in this library code (acceptable for ASP.NET Core)

### Modern C# Patterns (In Use)
- **Expression-Bodied Members**: Used for simple properties (e.g., `public string Authority => $"{AuthorityBase}/realms/{Realm}"`)
- **Target-Typed New**: Used when type is clear from context (e.g., `return new()`)
- **Switch Expressions**: Used for mapping (e.g., `SameSite switch { "Strict" => SameSiteMode.Strict, ... }`)
- **Collection Initializers**: Used extensively (e.g., `var directives = new List<string> { "default-src 'self'", ... }`)

### Code Quality Standards
- **Small, Focused Methods**: Each method has single responsibility
- **4-Space Indentation**: Standard C# convention
- **Remove Dead Code**: No commented-out code; delete unused code
- **No Backward Compatibility Hacks**: Unless explicitly required (e.g., no unused parameters, no `_varName` for removed vars)
- **DRY Principle**: Extract common logic to services or helper methods
