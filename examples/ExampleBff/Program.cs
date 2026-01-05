using affolterNET.Web.Bff.Extensions;
using affolterNET.Web.Core.Models;

var builder = WebApplication.CreateBuilder(args);

// Determine environment and auth mode
var isDev = builder.Environment.IsDevelopment();
var authMode = builder.Configuration.GetValue<AuthenticationMode>("AuthMode");
var appSettings = new AppSettings(isDev, authMode, isBff: true);

// Register BFF services using library pattern
var bffOptions = builder.Services.AddBffServices(appSettings, builder.Configuration);

// Add Razor Pages and Controllers
builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

// Configure BFF pipeline
app.ConfigureBffApp(bffOptions);

app.Run();
