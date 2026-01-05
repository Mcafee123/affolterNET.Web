FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy library projects
COPY affolterNET.Web.Core/affolterNET.Web.Core.csproj affolterNET.Web.Core/
COPY affolterNET.Web.Api/affolterNET.Web.Api.csproj affolterNET.Web.Api/
COPY examples/ExampleApi/ExampleApi.csproj examples/ExampleApi/

# Restore dependencies
RUN dotnet restore examples/ExampleApi/ExampleApi.csproj

# Copy source files
COPY README.md README.md
COPY affolterNET.Web.Core/ affolterNET.Web.Core/
COPY affolterNET.Web.Api/ affolterNET.Web.Api/
COPY examples/ExampleApi/ examples/ExampleApi/

# Build
WORKDIR /src/examples/ExampleApi
RUN dotnet build ExampleApi.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish ExampleApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Install ca-certificates for custom CA support
RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Entrypoint script to handle CA certificate
COPY examples/ExampleApi/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

ENTRYPOINT ["/docker-entrypoint.sh"]
