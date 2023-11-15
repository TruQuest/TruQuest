using Microsoft.Extensions.Logging;

using Application.Common.Interfaces;

namespace Infrastructure.Email;

internal class DummyEmailSender : IEmailSender
{
    private readonly ILogger<DummyEmailSender> _logger;

    public DummyEmailSender(ILogger<DummyEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendConfirmationEmail(string recipient, string subject, string body)
    {
        _logger.LogInformation($"Sending email to {recipient}:\n{subject}\n\t{body}");
        return Task.CompletedTask;
    }

    public Task ForwardEmail(string recipient, string filePath)
    {
        throw new NotImplementedException();
    }
}
