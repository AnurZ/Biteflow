namespace Market.Shared.Options;

public sealed class SmtpEmailOptions
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; } = false;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Biteflow";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
