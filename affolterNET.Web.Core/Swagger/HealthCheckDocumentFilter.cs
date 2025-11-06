using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace affolterNET.Web.Core.Swagger;

/// <summary>
/// Document filter to add health check endpoints to Swagger documentation
/// </summary>
public class HealthCheckDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var healthTag = new OpenApiTag
        {
            Name = "Health",
            Description = "Health check endpoints for monitoring application status"
        };

        swaggerDoc.Tags ??= new List<OpenApiTag>();
        swaggerDoc.Tags.Add(healthTag);

        // Add /health/startup endpoint
        AddHealthCheckEndpoint(
            swaggerDoc,
            "/health/startup",
            "Startup Health Check",
            "Checks application startup dependencies (e.g., Keycloak). Used by container orchestrators during startup."
        );

        // Add /health/live endpoint
        AddHealthCheckEndpoint(
            swaggerDoc,
            "/health/live",
            "Liveness Health Check",
            "Simple self-check to verify the application is alive and responsive. Used by container orchestrators to detect if the app needs to be restarted."
        );

        // Add /health/ready endpoint with JSON response
        AddHealthCheckEndpoint(
            swaggerDoc,
            "/health/ready",
            "Readiness Health Check",
            "Checks if the application is ready to accept traffic. Returns detailed JSON with all health check results. Used by container orchestrators and load balancers.",
            includeJsonResponse: true
        );
    }

    private static void AddHealthCheckEndpoint(
        OpenApiDocument swaggerDoc,
        string path,
        string summary,
        string description,
        bool includeJsonResponse = false)
    {
        var pathItem = new OpenApiPathItem
        {
            Operations = new Dictionary<OperationType, OpenApiOperation>
            {
                [OperationType.Get] = new OpenApiOperation
                {
                    Tags = new List<OpenApiTag> { new() { Name = "Health" } },
                    Summary = summary,
                    Description = description,
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse
                        {
                            Description = "Healthy",
                            Content = includeJsonResponse
                                ? new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema
                                        {
                                            Type = "object",
                                            Properties = new Dictionary<string, OpenApiSchema>
                                            {
                                                ["status"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("Healthy") },
                                                ["totalDuration"] = new OpenApiSchema { Type = "number", Format = "double" },
                                                ["checks"] = new OpenApiSchema
                                                {
                                                    Type = "array",
                                                    Items = new OpenApiSchema
                                                    {
                                                        Type = "object",
                                                        Properties = new Dictionary<string, OpenApiSchema>
                                                        {
                                                            ["name"] = new OpenApiSchema { Type = "string" },
                                                            ["status"] = new OpenApiSchema { Type = "string" },
                                                            ["description"] = new OpenApiSchema { Type = "string" },
                                                            ["duration"] = new OpenApiSchema { Type = "number", Format = "double" }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                : new Dictionary<string, OpenApiMediaType>
                                {
                                    ["text/plain"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = "string" }
                                    }
                                }
                        },
                        ["503"] = new OpenApiResponse
                        {
                            Description = "Unhealthy"
                        }
                    }
                }
            }
        };

        swaggerDoc.Paths.Add(path, pathItem);
    }
}
