using ACT.Application.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace ACT.Infrastructure.Services;

/// <summary>
/// Sends mail via SMTP using whatever provider's credentials are configured under "EmailSettings"
/// (SendGrid, Mailgun, Postmark, SES, etc. all speak SMTP). If no host is configured — the default
/// in local dev — the email is logged instead of sent, so the password-reset flow is fully
/// exercisable without real mail infrastructure. Never silently drops mail in an environment where
/// SmtpHost *is* set: a send failure there throws.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var host = _config["EmailSettings:SmtpHost"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning(
                "EmailSettings:SmtpHost is not configured — logging email instead of sending. " +
                "To: {ToEmail} Subject: {Subject}\n{Body}",
                toEmail, subject, htmlBody);
            return;
        }

        var port = int.TryParse(_config["EmailSettings:SmtpPort"], out var p) ? p : 587;
        var username = _config["EmailSettings:Username"];
        var password = _config["EmailSettings:Password"];
        var fromAddress = _config["EmailSettings:FromAddress"] ?? "no-reply@localhost";
        var fromName = _config["EmailSettings:FromName"] ?? "ACT";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        if (!string.IsNullOrWhiteSpace(username))
        {
            await client.AuthenticateAsync(username, password ?? string.Empty);
        }
        await client.SendAsync(message);
        await client.DisconnectAsync(quit: true);
    }
}
