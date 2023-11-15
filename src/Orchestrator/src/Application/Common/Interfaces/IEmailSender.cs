namespace Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendConfirmationEmail(string recipient, string subject, string body);
    Task ForwardEmail(string recipient, string filePath);
}
