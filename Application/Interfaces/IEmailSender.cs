namespace Application.Interfaces;

public interface IEmailSender
{
    public void SendEmail(string subject, string body);
}
