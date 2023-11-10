using System.Diagnostics;

using Microsoft.Extensions.Configuration;

using MimeKit;
using MailKit.Security;
using MailKit.Net.Smtp;

using Application;
using Application.Common.Interfaces;

namespace Infrastructure.User;

internal class EmailSender : IEmailSender
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _senderDisplayName;
    private readonly string _senderAddress;

    public EmailSender(IConfiguration configuration)
    {
        var smtpConfig = configuration.GetSection("Email:Smtp");
        _smtpHost = smtpConfig["Host"]!;
        _smtpPort = smtpConfig.GetValue<int>("Port");
        _smtpUsername = smtpConfig["Username"]!;
        _smtpPassword = smtpConfig["Password"]!;

        _senderDisplayName = configuration["Email:Sender:DisplayName"]!;
        _senderAddress = configuration["Email:Sender:Address"]!;
    }

    public async Task Send(string recipient, string subject, string body)
    {
        using var span = Telemetry.StartActivity($"{GetType().FullName}.{nameof(Send)}", kind: ActivityKind.Client)!;

        using var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_senderDisplayName, _senderAddress));
        email.To.Add(MailboxAddress.Parse(recipient));
        email.Subject = subject;
        var builder = new BodyBuilder();
        builder.HtmlBody = body;
        email.Body = builder.ToMessageBody();

        // @@TODO!!: Handle exceptions!
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_smtpUsername, _smtpPassword);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
