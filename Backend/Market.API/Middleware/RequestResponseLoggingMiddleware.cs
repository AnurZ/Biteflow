using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Market.API.Middlewares;

/// <summary>
/// Middleware that logs incoming HTTP requests and outgoing responses,
/// including duration, method, path, and status code.
/// </summary>
public sealed class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestResponseLoggingMiddleware> logger)
{
    private const int SlowRequestThresholdMs = 400; // 2 seconds

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        // Read request body (only for POST/PUT)
        string? requestBody = null;
        if (request.Method is "POST" or "PUT")
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Position = 0;
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
                var responseText = await new StreamReader(responseBody).ReadToEndAsync();
                responseBody.Seek(0, SeekOrigin.Begin);

                var logMessage = new StringBuilder()
                    .AppendLine("HTTP Request/Response Log:")
                    .AppendLine($"  Path: {request.Path}")
                    .AppendLine($"  Method: {request.Method}")
                    .AppendLine($"  Status: {context.Response.StatusCode}")
                    .AppendLine($"  Duration: {stopwatch.ElapsedMilliseconds} ms");

                if (!string.IsNullOrWhiteSpace(requestBody))
                    logMessage.AppendLine($"  Request Body: {RedactSensitiveBody(requestBody)}");

                if (!string.IsNullOrWhiteSpace(responseText))
                    logMessage.AppendLine($"  Response Body: {RedactSensitiveBody(responseText)}");

                var elapsed = stopwatch.ElapsedMilliseconds;
                if (elapsed > SlowRequestThresholdMs)
                {
                    logger.LogWarning("[SLOW REQUEST] {Path} took {Elapsed} ms", request.Path, elapsed);
                    await File.AppendAllTextAsync("Logs/slow-requests.log",
                        $"{DateTime.UtcNow:u} | {request.Path} | {elapsed} ms{Environment.NewLine}");
                }

                logger.LogInformation("{Log}", logMessage.ToString());

                await responseBody.CopyToAsync(originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }

        }
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
               key.Equals("token", StringComparison.OrdinalIgnoreCase) ||
               key.EndsWith("Password", StringComparison.OrdinalIgnoreCase) ||
               key.EndsWith("Token", StringComparison.OrdinalIgnoreCase);
    }
}
