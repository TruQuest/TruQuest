namespace Application.Common.Interfaces;

public interface IEmailForwarder
{
    Task FetchAndForward();
}
