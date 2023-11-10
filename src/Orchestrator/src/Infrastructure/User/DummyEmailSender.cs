using Microsoft.Extensions.Logging;

using Application.Common.Interfaces;

namespace Infrastructure.User;

internal class DummyEmailSender : IEmailSender
{
    private readonly ILogger<DummyEmailSender> _logger;

    public DummyEmailSender(ILogger<DummyEmailSender> logger)
    {
        _logger = logger;
    }

    public Task Send(string recipient, string subject, string body)
    {
        _logger.LogInformation($"Sending email to {recipient}:\n{subject}\n\t{body}");
        return Task.CompletedTask;
    }
}
