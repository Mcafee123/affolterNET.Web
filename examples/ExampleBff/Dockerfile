FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Build Vue SPA
FROM node:20-alpine AS spa-build
WORKDIR /spa
COPY examples/ExampleBff/ClientApp/package*.json ./
RUN npm install
COPY examples/ExampleBff/ClientApp/ ./
RUN npm run build

# Build .NET application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy library projects
COPY affolterNET.Web.Core/affolterNET.Web.Core.csproj affolterNET.Web.Core/
COPY affolterNET.Web.Bff/affolterNET.Web.Bff.csproj affolterNET.Web.Bff/
COPY examples/ExampleBff/ExampleBff.csproj examples/ExampleBff/

# Restore dependencies
RUN dotnet restore examples/ExampleBff/ExampleBff.csproj

# Copy source files
COPY README.md README.md
COPY affolterNET.Web.Core/ affolterNET.Web.Core/
COPY affolterNET.Web.Bff/ affolterNET.Web.Bff/
COPY examples/ExampleBff/ examples/ExampleBff/

# Build
WORKDIR /src/examples/ExampleBff
RUN dotnet build ExampleBff.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish ExampleBff.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Install ca-certificates for custom CA support
RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .
COPY --from=spa-build /wwwroot ./wwwroot

# Entrypoint script to handle CA certificate
COPY examples/ExampleBff/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

ENTRYPOINT ["/docker-entrypoint.sh"]
