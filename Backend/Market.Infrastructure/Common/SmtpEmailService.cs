using Market.Application.Abstractions;
using Market.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Market.Infrastructure.Common
{
    public sealed class SmtpEmailService(
        IOptions<SmtpEmailOptions> options,
        ILogger<SmtpEmailService> logger) : IEmailService
    {
        private readonly SmtpEmailOptions _options = options.Value;

        public async Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
        {
            if (!_options.Enabled)
            {
                logger.LogInformation("Email sending disabled. Skipping message to {Email}", toEmail);
                return;
            }

            if (string.IsNullOrWhiteSpace(_options.Host))
                throw new InvalidOperationException("Email:Host is not configured.");

            if (string.IsNullOrWhiteSpace(_options.FromAddress))
                throw new InvalidOperationException("Email:FromAddress is not configured.");

            if (string.IsNullOrWhiteSpace(toEmail))
                throw new InvalidOperationException("Recipient email is required.");

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromAddress, _options.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                client.Credentials = new NetworkCredential(_options.Username, _options.Password);
            }

            try
            {
                await client.SendMailAsync(message, ct);
                logger.LogInformation("SMTP email sent to {Email} via host {Host}:{Port}", toEmail, _options.Host, _options.Port);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SMTP email failed for {Email} via host {Host}:{Port}", toEmail, _options.Host, _options.Port);
                throw;
            }
        }
    }
}
