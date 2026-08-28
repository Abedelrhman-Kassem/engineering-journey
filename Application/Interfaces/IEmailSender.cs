namespace Application.Interfaces;

public interface IEmailSender
{
    public Task SendEmailAsync(string subject, string body);
}
