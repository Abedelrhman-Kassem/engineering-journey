using Application.Interfaces;

namespace Infrastructure.Services;

internal class EmailSender : IEmailSender
{
    public async Task SendEmailAsync(string subject, string body)
    {
        Console.WriteLine($"Sending email with subject: {subject} and body: {body}");
        await Task.CompletedTask;
    }
}
