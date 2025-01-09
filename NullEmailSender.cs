using Microsoft.AspNetCore.Identity.UI.Services;

public class NullEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        //no hace nada
    return Task.CompletedTask; 
    }
}

