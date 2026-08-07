using affolterNET.Web.Core.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace affolterNET.Web.Core.Extensions;

/// <summary>
/// The one logging setup for every application on this library.
///
/// Before this existed, each application hand-wrote its own <c>new LoggerConfiguration()</c> in
/// Program.cs — three applications, three different sets of levels and sinks, and not one of them
/// changeable without a rebuild and a deployment. Logging that can only be changed by shipping
/// code is useless exactly when it is needed (found 2026-08-07, while a production performance
/// problem could not be diagnosed because the readiness probe drowned the log).
///
/// Everything is now steerable from configuration, and therefore from environment variables:
///
///   Serilog__MinimumLevel__Default=Debug
///   Serilog__MinimumLevel__Override__Microsoft.AspNetCore=Information
///   Serilog__MinimumLevel__Override__affolterNET=Verbose
///
/// No code, no rebuild. See <see cref="UseAffolterNetSerilog"/> for the defaults that apply when
/// an application ships no Serilog section at all.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures Serilog from the application's <c>Serilog</c> configuration section and makes
    /// it the host logger. Call it right after <c>WebApplication.CreateBuilder</c>.
    ///
    /// The logger is created IMMEDIATELY (not deferred to host build) so that everything an
    /// application logs during service registration — option dumps, validation results — still
    /// lands in the same log.
    ///
    /// Without a Serilog section the behaviour matches what the applications did by hand:
    /// console output at Information, with the framework's per-request chatter turned down,
    /// because <see cref="ConfigureRequestLogging"/> replaces it with one summary line.
    /// </summary>
    public static WebApplicationBuilder UseAffolterNetSerilog(
        this WebApplicationBuilder builder, Action<LoggerConfiguration>? configure = null)
    {
        var config = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext();

        // Only fall back when the application configured nothing — otherwise a configured console
        // sink plus this one would print every line twice.
        if (!builder.Configuration.GetSection("Serilog:WriteTo").GetChildren().Any())
        {
            config = config.WriteTo.Console();
        }

        if (!builder.Configuration.GetSection("Serilog:MinimumLevel").GetChildren().Any())
        {
            config = config
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);
        }

        configure?.Invoke(config);

        Log.Logger = config.CreateLogger();
        builder.Host.UseSerilog();
        return builder;
    }

    /// <summary>
    /// One summary line per request, replacing the four lines ASP.NET Core writes by itself
    /// ("Request starting", "Executing endpoint", "Executed endpoint", "Request finished").
    ///
    /// The level is chosen per request, which is what makes noisy endpoints controllable without
    /// hiding anything that matters:
    ///
    ///   failed / 5xx      Error       — always visible
    ///   4xx               Warning     — always visible
    ///   excluded path     Verbose     — invisible at normal levels, back with
    ///                                   Serilog__MinimumLevel__Default=Verbose
    ///   everything else   Information
    ///
    /// That is the answer to "can I still see a failing health check": yes. Silence means the
    /// probe is healthy, never that it was suppressed. Which paths are quiet comes from
    /// <see cref="RequestLoggingOptions.ExcludePaths"/> (default <c>/health/</c>).
    ///
    /// ResponseBytes comes from <see cref="Middleware.ResponseSizeMiddleware"/>; it is the size
    /// the application WROTE. With response compression on, the wire carries less — the
    /// Content-Encoding in the same line says which of the two you are looking at.
    /// </summary>
    public static IApplicationBuilder ConfigureRequestLogging(
        this IApplicationBuilder app, RequestLoggingOptions options)
    {
        return app.UseSerilogRequestLogging(logging =>
        {
            logging.MessageTemplate =
                "{RequestMethod} {RequestPath} -> {StatusCode} in {Elapsed:0.0} ms, "
                + "{ResponseBytes} bytes{ResponseEncoding}";

            logging.GetLevel = (context, _, exception) =>
            {
                if (exception is not null || context.Response.StatusCode >= 500)
                {
                    return LogEventLevel.Error;
                }

                if (context.Response.StatusCode >= 400)
                {
                    return LogEventLevel.Warning;
                }

                return IsExcluded(context.Request.Path, options)
                    ? LogEventLevel.Verbose
                    : LogEventLevel.Information;
            };

            // ResponseBytes is NOT set here on purpose: this runs after the size middleware has
            // finished, so it would overwrite the counted value with the (usually absent)
            // Content-Length. The encoding belongs here — at this point the headers are final.
            logging.EnrichDiagnosticContext = (diagnostic, context) =>
                diagnostic.Set(
                    "ResponseEncoding",
                    context.Response.Headers.ContentEncoding.Count > 0
                        ? $", {context.Response.Headers.ContentEncoding}"
                        : string.Empty);
        });
    }

    internal static bool IsExcluded(PathString path, RequestLoggingOptions options) =>
        options.ExcludePaths.Any(prefix =>
            path.HasValue && path.Value!.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
}
