## General development conventions

### .NET Project Structure (Observed Pattern)
- **Solution Structure**: Multi-project solution with Core, Api, and Bff libraries
  - `affolterNET.Web.Core`: Shared components, middleware, services
  - `affolterNET.Web.Api`: API-specific authentication (JWT Bearer)
  - `affolterNET.Web.Bff`: BFF pattern with YARP reverse proxy
- **Folder Organization**: Consistent folders across projects
  - `Authorization/`: Authorization policies and handlers
  - `Configuration/`: Options and configuration classes
  - `Controllers/`: API controllers
  - `Extensions/`: Extension methods for DI and middleware
  - `Middleware/`: Custom middleware components
  - `Models/`: DTOs and domain models
  - `Services/`: Business logic and service implementations
- **Namespace Conventions**: Match folder structure (e.g., `affolterNET.Web.Core.Middleware`)

### NuGet Package Management
- **Package Properties**: Defined in .csproj files
  - `<PackageId>`, `<Version>`, `<Authors>`, `<Description>`
  - `<PackageProjectUrl>`, `<RepositoryUrl>`, `<PackageTags>`
  - `<PackageLicenseExpression>MIT</PackageLicenseExpression>`
- **Version Management**: Semantic versioning (e.g., `0.3.13`)
- **GeneratePackageOnBuild**: Set to `true` for automatic packing
- **Include README**: Bundle README.md in packages via `<None Include="../README.md" Pack="true" PackagePath="\" />`

### Configuration Management
- **Configuration Sections**: Structured, nested sections in appsettings.json
  - `affolterNET:Web:Auth:Provider`: Authentication provider settings
  - `affolterNET:Web:Bff:Options`: BFF-specific options
  - `affolterNET:ReverseProxy`: YARP configuration
- **Options Pattern**: All configuration uses `IConfigurableOptions<T>` with static `SectionName` property
- **Sensible Attribute**: Mark sensitive properties (e.g., `[Sensible] public string ClientSecret`)
- **No Secrets in Code**: Never commit API keys, client secrets, or connection strings
- **Environment Variables**: Support environment variable overrides for all config

### Version Control Practices
- **Clear Commit Messages**: Descriptive commits explaining the "why"
- **Feature Branches**: Use `develop` branch for active development
- **Main Branch**: `main` for stable releases
- **No Generated Files**: obj/, bin/, and .DotSettings.user excluded via .gitignore
- **CHANGELOG.md**: Not observed in this project but recommended for tracking changes

### Dependency Management
- **Framework References**: Use `<FrameworkReference Include="Microsoft.AspNetCore.App" />` instead of explicit packages when possible
- **Project References**: Use `<ProjectReference>` for inter-project dependencies within solution
- **Minimal Dependencies**: Only include necessary packages; keep versions aligned
- **Latest Stable Versions**: Use latest stable Microsoft packages (e.g., 9.0.9 for .NET 9)

### CI/CD Pipeline (GitHub Actions)
- **Build Workflow**: Automated build, test, pack, and publish on pushes/PRs
- **NuGet Publishing**: Automatic publishing to NuGet.org on releases
- **Version Tags**: Use semantic version tags (e.g., `v1.0.0`) for releases
- **NuGet API Key**: Stored in GitHub secrets as `NUGET_API_KEY`

### Testing Requirements
- **Test Framework**: xUnit (standard for .NET)
- **Test Projects**: Separate test projects with `.Tests` suffix (not currently in this solution but recommended)
- **Coverage**: Unit tests for services, integration tests for middleware

### Documentation Standards
- **README.md**: Comprehensive documentation with diagrams, usage examples, configuration
- **XML Comments**: All public APIs documented for IntelliSense
- **Architectural Diagrams**: Use ASCII art or markdown tables for architecture overviews (seen in README)
