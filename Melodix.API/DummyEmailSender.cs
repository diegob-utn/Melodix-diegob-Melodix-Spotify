using Microsoft.AspNetCore.Identity.UI.Services;

public class DummyEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Aquí puedes registrar en consola, log, etc.
        return Task.CompletedTask;
    }
}