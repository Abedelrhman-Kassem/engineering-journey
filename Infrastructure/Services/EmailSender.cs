using Application.Interfaces;

namespace Infrastructure.Services;

internal sealed class EmailSender : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string body, CancellationToken cancellationToken)
    {
        await Task.Delay(3000, cancellationToken);
        Console.WriteLine($"Sending email to {email} with subject: {subject} and body: {body}");
    }
}
