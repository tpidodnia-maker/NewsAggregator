using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using NewsAggregator.Core.Interfaces;

namespace NewsAggregator.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendPasswordResetAsync(string toEmail, string username, string resetLink)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_config["EmailSettings:From"]));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Сброс пароля — NewsAggregator";

        message.Body = new TextPart("html")
        {
            Text = $"""
                <h2>Привет, {username}!</h2>
                <p>Для сброса пароля нажмите кнопку ниже:</p>
                <a href="{resetLink}" style="
                    background:#2563eb; color:white; padding:12px 24px;
                    border-radius:8px; text-decoration:none; display:inline-block;">
                    Сбросить пароль
                </a>
                <p>Ссылка действительна 1 час.</p>
                <p>Если вы не запрашивали сброс — проигнорируйте письмо.</p>
                """
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
            _config["EmailSettings:Host"],
            int.Parse(_config["EmailSettings:Port"]!),
            SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(
            _config["EmailSettings:Username"],
            _config["EmailSettings:Password"]);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }
}