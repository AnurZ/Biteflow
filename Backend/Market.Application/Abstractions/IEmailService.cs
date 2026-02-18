namespace Market.Application.Abstractions;

public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default);
}
