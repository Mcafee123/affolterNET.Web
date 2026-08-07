using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Serilog;

namespace affolterNET.Web.Core.Middleware;

/// <summary>
/// Counts the bytes an application writes and hands the number to Serilog's request logging as
/// <c>ResponseBytes</c>.
///
/// Why it exists: <c>UseSerilogRequestLogging</c> reports method, path, status and duration, but
/// not the size — and <c>Content-Length</c> is usually absent because responses are chunked.
/// Without the size there is no way to tell an expensive query from an oversized payload. On
/// ShelterBox both were true at once: 8 seconds in storage AND 7.9 MB of uncompressed JSON in
/// one answer (measured 2026-08-07); the size is what proved the second half.
///
/// It must sit DIRECTLY inside <c>UseSerilogRequestLogging</c>: the counter has to finish before
/// the log line is written, and it must wrap every downstream writer.
///
/// The count is what the application wrote. With response compression enabled the wire carries
/// less — the Content-Encoding in the same log line says which of the two you are reading.
/// </summary>
public class ResponseSizeMiddleware(RequestDelegate next, IDiagnosticContext diagnosticContext)
{
    public async Task Invoke(HttpContext context)
    {
        var originalBody = context.Features.Get<IHttpResponseBodyFeature>();
        if (originalBody is null)
        {
            await next(context);
            return;
        }

        // Replacing the FEATURE rather than Response.Body keeps SendFile working — static files
        // would otherwise bypass the counter entirely, and report zero.
        var counter = new CountingStream(originalBody.Stream);
        context.Features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(counter, originalBody));

        try
        {
            await next(context);
        }
        finally
        {
            context.Features.Set(originalBody);
            diagnosticContext.Set("ResponseBytes", counter.BytesWritten);
        }
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
