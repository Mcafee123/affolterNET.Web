using affolterNET.Web.Api.Extensions;
using affolterNET.Web.Core.Extensions;
using affolterNET.Web.Core.Models;

var builder = WebApplication.CreateBuilder(args);

// Logging: ONE line, and every level is steerable from the Serilog section in
// appsettings.json — and therefore from environment variables, without a rebuild:
//
//   Serilog__MinimumLevel__Default=Debug
//   Serilog__MinimumLevel__Override__Microsoft=Information
//
// Do NOT hand-write `new LoggerConfiguration()` here. Five applications did, ended up
// with five different setups, and none of them could be changed without a deployment —
// two even had a carefully written Serilog section that was never read (2026-08-07).
builder.UseAffolterNetSerilog();

// Determine environment and auth mode
var isDev = builder.Environment.IsDevelopment();
var authMode = builder.Configuration.GetValue<AuthenticationMode>("AuthMode");
var appSettings = new AppSettings(isDev, authMode);

// Register API services using library pattern
var apiOptions = builder.Services.AddApiServices(appSettings, builder.Configuration);

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configure API pipeline
app.ConfigureApiApp(apiOptions);

// Map controller endpoints
app.MapControllers();

app.Run();
