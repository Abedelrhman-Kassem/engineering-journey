namespace Application.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string subject, string body);
}
