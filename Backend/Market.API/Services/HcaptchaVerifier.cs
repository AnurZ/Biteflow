using System.Net.Http.Headers;
using System.Text.Json;
using Market.API.Options;
using Market.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Market.API.Services;

public sealed class HcaptchaVerifier : ICaptchaVerifier
{
    private const string TestSecret = "0x0000000000000000000000000000000000000000";
    private const string TestBypassToken = "10000000-aaaa-bbbb-cccc-000000000001";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptionsMonitor<CaptchaOptions> _options;
    private readonly ILogger<HcaptchaVerifier> _logger;

    public HcaptchaVerifier(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IOptionsMonitor<CaptchaOptions> options,
        ILogger<HcaptchaVerifier> logger)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _options = options;
        _logger = logger;
    }

    public async Task<bool> VerifyAsync(string token, CancellationToken ct = default)
    {
        var opt = _options.CurrentValue;
        if (!opt.Enabled)
        {
            _logger.LogInformation("Captcha verification skipped (disabled).");
            return true;
        }

        if (token == TestBypassToken)
        {
            _logger.LogInformation("Captcha verification bypassed using test token.");
            return true;
        }

        if (string.IsNullOrWhiteSpace(opt.SecretKey))
        {
            _logger.LogWarning("Captcha secret key not configured.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Captcha token missing.");
            return false;
        }

        var client = _httpClientFactory.CreateClient(nameof(HcaptchaVerifier));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("response", token),
            new KeyValuePair<string, string>("secret", opt.SecretKey),
            new KeyValuePair<string, string>("remoteip", GetRemoteIp() ?? string.Empty)
        });

        try
        {
            using var response = await client.PostAsync(opt.VerifyEndpoint, form, ct);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            var result = await JsonSerializer.DeserializeAsync<HcaptchaResponse>(stream, cancellationToken: ct);

            if (result?.Success == true)
            {
                return true;
            }

            _logger.LogWarning("Captcha verification failed. Errors: {Errors}", string.Join(", ", result?.ErrorCodes ?? Array.Empty<string>()));
            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Captcha verification request failed.");
            return false;
        }
    }

    private string? GetRemoteIp()
    {
        return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }

    private sealed class HcaptchaResponse
    {
        public bool Success { get; set; }
        public string? Challenge_ts { get; set; }
        public string? Hostname { get; set; }
        public string[]? ErrorCodes { get; set; }
    }
}
