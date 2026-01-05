using affolterNET.Web.Api.Extensions;
using affolterNET.Web.Core.Models;

var builder = WebApplication.CreateBuilder(args);

// Determine environment and auth mode
var isDev = builder.Environment.IsDevelopment();
var authMode = builder.Configuration.GetValue<AuthenticationMode>("AuthMode");
var appSettings = new AppSettings(isDev, authMode, isBff: false);

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
