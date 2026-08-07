using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using affolterNET.Web.Core.Configuration;

namespace affolterNET.Web.Core.Middleware;

public class RequestLoggingMiddleware(
    RequestDelegate next,
    IOptionsMonitor<RequestLoggingOptions> options,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        if (IsExcluded(path))
        {
            await next(context);
            return;
        }

        logger.LogInformation("Request: {Method} {Path}", context.Request.Method, path);

        // How long the server took and how much it wrote. Without these two numbers a slow page
        // cannot be attributed: a request that takes three seconds in the browser may spend them
        // in the server, on the wire, or in the client — and guessing wrong costs days
        // (ShelterBox 2026-08-07).
        var startedAt = Stopwatch.GetTimestamp();
        var originalBody = context.Features.Get<IHttpResponseBodyFeature>();
        var counter = originalBody is null ? null : new CountingStream(originalBody.Stream);
        if (counter is not null && originalBody is not null)
        {
            // Replacing the FEATURE rather than Response.Body keeps SendFile working — static
            // files would otherwise bypass the counter (and worse, bypass the wrapper entirely).
            context.Features.Set<IHttpResponseBodyFeature>(
                new StreamResponseBodyFeature(counter, originalBody));
        }

        try
        {
            await next(context);
        }
        finally
        {
            if (originalBody is not null)
            {
                context.Features.Set(originalBody);
            }

            var elapsed = Stopwatch.GetElapsedTime(startedAt);
            var endpoint = context.GetEndpoint();

            // The byte count is what the application WROTE. When response compression is on, the
            // wire carries less — the Content-Encoding tells you which of the two you are seeing.
            logger.LogInformation(
                "Response: {Method} {Path} -> {StatusCode} in {ElapsedMs} ms, {Bytes} bytes written"
                + "{Encoding} (endpoint {Endpoint})",
                context.Request.Method,
                path,
                context.Response.StatusCode,
                (int)elapsed.TotalMilliseconds,
                counter?.BytesWritten ?? context.Response.ContentLength ?? 0,
                context.Response.Headers.ContentEncoding.Count > 0
                    ? $", {context.Response.Headers.ContentEncoding}"
                    : string.Empty,
                endpoint?.DisplayName ?? "None");
        }
    }

    private bool IsExcluded(string path)
    {
        var excludePaths = options.CurrentValue.ExcludePaths;
        foreach (var prefix in excludePaths)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Counts what is written through it and passes everything on unchanged. Only the write
    /// paths are overridden — every other member delegates, so the stream behaves exactly like
    /// the one it wraps.
    /// </summary>
    private sealed class CountingStream(Stream inner) : Stream
    {
        public long BytesWritten { get; private set; }

        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => inner.CanSeek;
        public override bool CanWrite => inner.CanWrite;
        public override long Length => inner.Length;

        public override long Position
        {
            get => inner.Position;
            set => inner.Position = value;
        }

        public override void Flush() => inner.Flush();

        public override Task FlushAsync(CancellationToken cancellationToken) =>
            inner.FlushAsync(cancellationToken);

        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

        public override void SetLength(long value) => inner.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
        {
            BytesWritten += count;
            inner.Write(buffer, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            BytesWritten += buffer.Length;
            inner.Write(buffer);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            BytesWritten += count;
            return inner.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            BytesWritten += buffer.Length;
            return inner.WriteAsync(buffer, cancellationToken);
        }
    }
}
