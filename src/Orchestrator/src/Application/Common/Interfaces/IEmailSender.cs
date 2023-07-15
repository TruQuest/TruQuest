namespace Application.Common.Interfaces;

public interface IEmailSender
{
    Task Send(string recipient, string body);
}
