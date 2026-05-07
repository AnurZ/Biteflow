using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Market.API.Middlewares;

/// <summary>
/// Middleware that logs incoming HTTP requests and outgoing responses,
/// including duration, method, path, status code, and correlation id.
/// </summary>
public sealed class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    IWebHostEnvironment environment,
    ILogger<RequestResponseLoggingMiddleware> logger)
{
    private const int SlowRequestThresholdMs = 400; // 400 ms

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;
        var shouldLogBodies = environment.IsDevelopment();
        var shouldCaptureRequestBody = shouldLogBodies &&
                                       request.Method is "POST" or "PUT" &&
                                       IsLoggableContentType(request.ContentType);

        string? requestBody = null;
        if (shouldCaptureRequestBody)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        if (!shouldLogBodies)
        {
            try
            {
                await next(context);
            }
            finally
            {
                stopwatch.Stop();
                LogRequest(context, stopwatch.ElapsedMilliseconds);
            }

            return;
        }

        var originalBodyStream = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            try
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseText = IsLoggableContentType(context.Response.ContentType)
                    ? await new StreamReader(responseBody).ReadToEndAsync()
                    : null;
                responseBody.Seek(0, SeekOrigin.Begin);

                LogRequest(context, stopwatch.ElapsedMilliseconds, requestBody, responseText);

                await responseBody.CopyToAsync(originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }

    private void LogRequest(HttpContext context, long elapsed)
        => LogRequest(context, elapsed, requestBody: null, responseBody: null);

    private void LogRequest(HttpContext context, long elapsed, string? requestBody, string? responseBody)
    {
        var request = context.Request;
        var correlationId = Activity.Current?.Id ?? context.TraceIdentifier;

        if (elapsed > SlowRequestThresholdMs)
        {
            logger.LogWarning(
                "[SLOW REQUEST] {Method} {Path} took {Elapsed} ms. CorrelationId={CorrelationId}",
                request.Method,
                request.Path,
                elapsed,
                correlationId);
        }

        var redactedRequestBody = string.IsNullOrWhiteSpace(requestBody)
            ? null
            : RedactSensitiveBody(requestBody);
        var redactedResponseBody = string.IsNullOrWhiteSpace(responseBody)
            ? null
            : RedactSensitiveBody(responseBody);

        if (redactedRequestBody is null && redactedResponseBody is null)
        {
            logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {Elapsed} ms. CorrelationId={CorrelationId}",
                request.Method,
                request.Path,
                context.Response.StatusCode,
                elapsed,
                correlationId);
            return;
        }

        logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {Elapsed} ms. CorrelationId={CorrelationId}. RequestBody={RequestBody}. ResponseBody={ResponseBody}",
            request.Method,
            request.Path,
            context.Response.StatusCode,
            elapsed,
            correlationId,
            redactedRequestBody,
            redactedResponseBody);
    }

    private static bool IsLoggableContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        var mediaType = contentType.Split(';', 2)[0].Trim();
        return mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) ||
               mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase) ||
               mediaType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) ||
               mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase);
    }

    private static string RedactSensitiveBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return body;
        }

        var trimmed = body.TrimStart();
        if (trimmed.StartsWith('{') || trimmed.StartsWith('['))
        {
            try
            {
                var node = JsonNode.Parse(body);
                RedactJsonNode(node);
                return node?.ToJsonString(new JsonSerializerOptions { WriteIndented = false }) ?? body;
            }
            catch (JsonException)
            {
                return body;
            }
        }

        if (body.Contains('='))
        {
            return string.Join("&", body.Split('&').Select(part =>
            {
                var separatorIndex = part.IndexOf('=');
                if (separatorIndex < 0)
                {
                    return part;
                }

                var key = Uri.UnescapeDataString(part[..separatorIndex].Replace('+', ' '));
                return IsSensitiveKey(key) ? $"{part[..separatorIndex]}=[REDACTED]" : part;
            }));
        }

        return body;
    }

    private static void RedactJsonNode(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            foreach (var property in obj.ToList())
            {
                if (IsSensitiveKey(property.Key))
                {
                    obj[property.Key] = "[REDACTED]";
                    continue;
                }

                RedactJsonNode(property.Value);
            }

            return;
        }

        if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                RedactJsonNode(item);
            }
        }
    }

    private static bool IsSensitiveKey(string key)
    {
        return key.Equals("password", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("newPassword", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("confirmPassword", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("refreshToken", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("accessToken", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("adminPassword", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("secret", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("token", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("email", StringComparison.OrdinalIgnoreCase) ||
               key.EndsWith("Password", StringComparison.OrdinalIgnoreCase) ||
               key.EndsWith("Token", StringComparison.OrdinalIgnoreCase);
    }
}
