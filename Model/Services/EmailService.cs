using System.Text.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PortfolioAPI.Model.Services;

public class EmailService(IConfiguration configuration, ILogger<EmailService> logger)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<EmailService> _logger = logger;

    public async Task SendContactEmailAsync(string name, string email, string message)
    {
        try
        {
            var apiKey = _configuration["BrevoApiKey"];
            var senderEmail = _configuration["SenderEmail"];
            var recipientEmail = _configuration["RecipientEmail"];

            var emailPayload = new
            {
                sender = new { email = senderEmail, name = "Portfolio Contact" },
                to = new[] { new { email = recipientEmail } },
                subject = $"New Contact Message from {name}",
                htmlContent = $@"
                    <h3>New Contact Message</h3>
                    <p><strong>From:</strong> {name}</p>
                    <p><strong>Email:</strong> {email}</p>
                    <p><strong>Message:</strong></p>
                    <p>{message}</p>
                ",
            };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            var jsonBody = JsonSerializer.Serialize(emailPayload);
            using var content = new StringContent(
                jsonBody,
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Failed to send contact email. Status Code: {StatusCode}, Response: {Response}",
                    response.StatusCode,
                    responseContent
                );
                throw new Exception(
                    $"Failed to send contact email. Status Code: {response.StatusCode}, Response: {responseContent}"
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending contact email.");
            throw;
        }
    }
}
