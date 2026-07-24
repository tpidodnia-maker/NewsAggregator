namespace NewsAggregator.Core.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetAsync(string toEmail, string username, string resetLink);
}