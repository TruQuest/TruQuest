using Microsoft.Extensions.Logging;

using Application.Common.Interfaces;

namespace Infrastructure.User;

internal class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ILogger<EmailSender> logger)
    {
        _logger = logger;
    }

    public async Task Send(string recipient, string body)
    {
        _logger.LogInformation($"Sending email to {recipient}:\n{body}");
    }
}
