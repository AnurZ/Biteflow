using System.Text;
using Market.API.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Market.Tests.AuthTests;

public sealed class RequestResponseLoggingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_InProduction_LogsMetadataWithoutRequestOrResponseBodies()
    {
        var logger = new CapturingLogger<RequestResponseLoggingMiddleware>();
        var context = CreateContext(
            "/connect/token",
            "POST",
            "application/json",
            """{"password":"plain-password","refreshToken":"refresh-token"}""");

        var middleware = new RequestResponseLoggingMiddleware(
            async ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status200OK;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync("""{"accessToken":"access-token"}""");
            },
            new TestWebHostEnvironment("Production"),
            logger);

        await middleware.InvokeAsync(context);

        var info = Assert.Single(logger.Entries.Where(x => x.Level == LogLevel.Information));
        Assert.Contains("HTTP POST /connect/token responded 200", info.Message);
        Assert.Contains("CorrelationId=trace-production", info.Message);
        Assert.DoesNotContain("RequestBody", info.Message);
        Assert.DoesNotContain("ResponseBody", info.Message);
        Assert.DoesNotContain("plain-password", info.Message);
        Assert.DoesNotContain("refresh-token", info.Message);
        Assert.DoesNotContain("access-token", info.Message);
    }

    [Fact]
    public async Task InvokeAsync_InDevelopment_LogsTextBodiesWithSensitiveFieldsRedacted()
    {
        var logger = new CapturingLogger<RequestResponseLoggingMiddleware>();
        var context = CreateContext(
            "/activate/confirm",
            "POST",
            "application/json",
            """{"email":"owner@example.com","adminPassword":"admin-pass","nested":{"token":"setup-token"},"name":"Biteflow"}""");

        var middleware = new RequestResponseLoggingMiddleware(
            async ctx =>
            {
                using var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8, leaveOpen: true);
                var requestBody = await reader.ReadToEndAsync();
                Assert.Contains("admin-pass", requestBody);

                ctx.Response.StatusCode = StatusCodes.Status200OK;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync("""{"accessToken":"access-token","ok":true}""");
            },
            new TestWebHostEnvironment("Development"),
            logger);

        await middleware.InvokeAsync(context);

        var info = Assert.Single(logger.Entries.Where(x => x.Level == LogLevel.Information));
        Assert.Contains("RequestBody=", info.Message);
        Assert.Contains("ResponseBody=", info.Message);
        Assert.Contains(@"""adminPassword"":""[REDACTED]""", info.Message);
        Assert.Contains(@"""email"":""[REDACTED]""", info.Message);
        Assert.Contains(@"""token"":""[REDACTED]""", info.Message);
        Assert.Contains(@"""accessToken"":""[REDACTED]""", info.Message);
        Assert.Contains(@"""name"":""Biteflow""", info.Message);
        Assert.DoesNotContain("owner@example.com", info.Message);
        Assert.DoesNotContain("admin-pass", info.Message);
        Assert.DoesNotContain("setup-token", info.Message);
        Assert.DoesNotContain("access-token", info.Message);
    }

    [Fact]
    public async Task InvokeAsync_InDevelopment_RedactsFormUrlEncodedBodies()
    {
        var logger = new CapturingLogger<RequestResponseLoggingMiddleware>();
        var context = CreateContext(
            "/connect/token",
            "POST",
            "application/x-www-form-urlencoded",
            "username=user&password=plain-password&refreshToken=refresh-token&scope=openid");

        var middleware = new RequestResponseLoggingMiddleware(
            async ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status200OK;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync("""{"ok":true}""");
            },
            new TestWebHostEnvironment("Development"),
            logger);

        await middleware.InvokeAsync(context);

        var info = Assert.Single(logger.Entries.Where(x => x.Level == LogLevel.Information));
        Assert.Contains("password=[REDACTED]", info.Message);
        Assert.Contains("refreshToken=[REDACTED]", info.Message);
        Assert.Contains("username=user", info.Message);
        Assert.DoesNotContain("plain-password", info.Message);
        Assert.DoesNotContain("refresh-token", info.Message);
    }

    [Fact]
    public async Task InvokeAsync_InDevelopment_DoesNotLogMultipartOrBinaryBodies()
    {
        var logger = new CapturingLogger<RequestResponseLoggingMiddleware>();
        var context = CreateContext(
            "/files",
            "POST",
            "multipart/form-data; boundary=abc123",
            "file-bytes-and-password=plain-password");

        var middleware = new RequestResponseLoggingMiddleware(
            async ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status201Created;
                ctx.Response.ContentType = "application/octet-stream";
                await ctx.Response.Body.WriteAsync(Encoding.UTF8.GetBytes("binary-access-token"));
            },
            new TestWebHostEnvironment("Development"),
            logger);

        await middleware.InvokeAsync(context);

        var info = Assert.Single(logger.Entries.Where(x => x.Level == LogLevel.Information));
        Assert.DoesNotContain("RequestBody", info.Message);
        Assert.DoesNotContain("ResponseBody", info.Message);
        Assert.DoesNotContain("plain-password", info.Message);
        Assert.DoesNotContain("binary-access-token", info.Message);
    }

    [Fact]
    public async Task InvokeAsync_ForSlowRequest_LogsWarningWithoutWritingSeparateSlowRequestFile()
    {
        var originalCurrentDirectory = Directory.GetCurrentDirectory();
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"biteflow-logging-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            Directory.SetCurrentDirectory(tempDirectory);
            var logger = new CapturingLogger<RequestResponseLoggingMiddleware>();
            var context = CreateContext("/slow", "GET", contentType: null, body: null);

            var middleware = new RequestResponseLoggingMiddleware(
                async ctx =>
                {
                    await Task.Delay(425);
                    ctx.Response.StatusCode = StatusCodes.Status204NoContent;
                },
                new TestWebHostEnvironment("Production"),
                logger);

            await middleware.InvokeAsync(context);

            var warning = Assert.Single(logger.Entries.Where(x => x.Level == LogLevel.Warning));
            Assert.Contains("[SLOW REQUEST]", warning.Message);
            Assert.Contains("GET", warning.Message);
            Assert.Contains("/slow", warning.Message);
            Assert.Contains("CorrelationId=trace-production", warning.Message);
            Assert.False(File.Exists(Path.Combine(tempDirectory, "Logs", "slow-requests.log")));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalCurrentDirectory);
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    private static DefaultHttpContext CreateContext(string path, string method, string? contentType, string? body)
    {
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "trace-production";
        context.Request.Path = path;
        context.Request.Method = method;
        context.Response.Body = new MemoryStream();

        if (contentType is not null)
        {
            context.Request.ContentType = contentType;
        }

        if (body is not null)
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            context.Request.Body = new MemoryStream(bytes);
            context.Request.ContentLength = bytes.Length;
        }

        return context;
    }

    private sealed class TestWebHostEnvironment(string environmentName) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Market.Tests";
        public string WebRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
            => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(logLevel, formatter(state, exception)));
        }
    }

    private sealed record LogEntry(LogLevel Level, string Message);

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}
