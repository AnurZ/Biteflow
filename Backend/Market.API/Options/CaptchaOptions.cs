using System.ComponentModel.DataAnnotations;

namespace Market.API.Options;

public sealed class CaptchaOptions
{
    public const string SectionName = "Captcha";

    [Required]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string VerifyEndpoint { get; set; } = "https://hcaptcha.com/siteverify";

    public bool Enabled { get; set; } = true;
}
