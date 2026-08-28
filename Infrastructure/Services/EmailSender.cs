using Application.Interfaces;

namespace Infrastructure.Services;

internal class EmailSender : IEmailSender
{
    public void SendEmail(string subject, string body)
    {
        Console.WriteLine($"Sending email with subject: {subject} and body: {body}");
    }
}
