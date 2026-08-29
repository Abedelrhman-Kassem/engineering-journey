using Application.Interfaces;

namespace Infrastructure.Services;

internal sealed class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string subject, string body, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Sending email with subject: {subject} and body: {body}");
        return Task.CompletedTask;
    }
}
