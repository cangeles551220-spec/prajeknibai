using System.Net;
using System.Net.Mail;

namespace TechServe.Web.Services;

public interface IEmailSender
{
    Task<bool> SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default);
}

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration configuration;
    private readonly ILogger<SmtpEmailSender> logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task<bool> SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        var host = configuration["Email:SmtpHost"];
        var fromAddress = configuration["Email:FromAddress"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromAddress))
        {
            return false;
        }

        using var message = new MailMessage(fromAddress, recipient, subject, body);
        using var client = new SmtpClient(host, configuration.GetValue("Email:SmtpPort", 25))
        {
            EnableSsl = configuration.GetValue("Email:EnableSsl", false),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        var username = configuration["Email:SmtpUsername"];
        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(username, configuration["Email:SmtpPassword"] ?? string.Empty);
        }

        try
        {
            await client.SendMailAsync(message, cancellationToken);
            return true;
        }
        catch (SmtpException exception)
        {
            logger.LogError(exception, "Password reset email delivery failed.");
            return false;
        }
    }
}