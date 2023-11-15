using Application.Common.Interfaces;

namespace Infrastructure.Email;

internal class DummyEmailForwarder : IEmailForwarder
{
    public Task FetchAndForward() => Task.CompletedTask;
}
