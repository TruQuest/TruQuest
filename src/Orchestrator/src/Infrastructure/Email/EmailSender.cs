using System.Diagnostics;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using MimeKit;
using MimeKit.Utils;
using MailKit.Security;
using MailKit.Net.Smtp;

using Application.Common.Interfaces;
using Application.Common.Monitoring;

namespace Infrastructure.Email;

internal class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;

    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _senderDisplayName;
    private readonly string _senderAddress;

    public EmailSender(ILogger<EmailSender> logger, IConfiguration configuration)
    {
        _logger = logger;

        var smtpConfig = configuration.GetSection("Email:Smtp");
        _smtpHost = smtpConfig["Host"]!;
        _smtpPort = smtpConfig.GetValue<int>("Port");
        _smtpUsername = smtpConfig["Username"]!;
        _smtpPassword = smtpConfig["Password"]!;

        _senderDisplayName = configuration["Email:Sender:DisplayName"]!;
        _senderAddress = configuration["Email:Sender:Address"]!;
    }

    private async Task _sendEmail(MimeMessage email)
    {
        // @@TODO!!: Handle exceptions!
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_smtpUsername, _smtpPassword);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    public async Task SendConfirmationEmail(string recipient, string subject, string body)
    {
        using var span = Telemetry.StartActivity($"{GetType().FullName}.{nameof(SendConfirmationEmail)}", kind: ActivityKind.Client)!;

        using var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_senderDisplayName, _senderAddress));
        email.To.Add(MailboxAddress.Parse(recipient));
        email.Subject = subject;
        var builder = new BodyBuilder();
        builder.HtmlBody = body;
        email.Body = builder.ToMessageBody();

        await _sendEmail(email);

        _logger.LogInformation($"Sent confirmation email to {LogMessagePlaceholders.Email}", recipient);
    }

    public async Task ForwardEmail(string recipient, string filePath)
    {
        using var span = Telemetry.StartActivity($"{GetType().FullName}.{nameof(ForwardEmail)}", kind: ActivityKind.Client)!;

        using var receivedEmail = await MimeMessage.LoadAsync(filePath);
        using var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_senderDisplayName, _senderAddress));
        email.ResentFrom.Add(receivedEmail.From.First());
        email.ResentReplyTo.Add(receivedEmail.From.First());
        email.ResentTo.Add(MailboxAddress.Parse(recipient));
        email.ResentMessageId = MimeUtils.GenerateMessageId();
        email.ResentDate = DateTimeOffset.Now;
        email.Subject = receivedEmail.Subject;
        email.Body = receivedEmail.Body;

        await _sendEmail(email);
    }
}
