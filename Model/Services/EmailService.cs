using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PortfolioAPI.Model.Services;

public class EmailService(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public async Task SendContactEmailAsync(string name, string email, string message)
    {
        var settings = _configuration.GetSection("EmailSettings");

        string? senderEmail = settings["SenderEmail"];
        string? senderPassword = settings["SenderPassword"];
        string? recipientEmail = settings["RecipientEmail"];
        string? smtpServer = settings["SmtpServer"];
        string? appName = settings["AppName"];

        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(appName, senderEmail!));
        emailMessage.To.Add(new MailboxAddress(appName, recipientEmail!));
        emailMessage.ReplyTo.Add(new MailboxAddress(name, email));
        emailMessage.Subject = $"New Contact Message from {name}";
        emailMessage.Body = new TextPart("plain") { Text = $"From: {name} ({email})\n\n{message}" };

        //declaring a fallback port if 587 fails to connect, as some ISPs block this port. The fallback port is 465.
        int primaryPort = int.Parse(settings["SmtpPort"]!);
        const int fallbackPort = 465;

        var portsToTry = new[]
        {
            new
            {
                Port = primaryPort,
                Options = primaryPort == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls,
            },
            new
            {
                Port = fallbackPort,
                Options = fallbackPort == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls,
            },
        };

        // checking if delivery is successful
        bool isDeliverySuccessful = false;

        List<string> errorLogs = [];

        foreach (var port in portsToTry)
        {
            try
            {
                Console.WriteLine($"Attempting to send email using port {port.Port}...");

                using var client = new SmtpClient();

                client.Timeout = 8000; // Set a timeout of 8 seconds

                await client.ConnectAsync(smtpServer!, port: port.Port, options: port.Options);
                await client.AuthenticateAsync(senderEmail!, senderPassword!);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);

                isDeliverySuccessful = true;
                Console.WriteLine($"Email sent successfully using port {port.Port}.");
                break; // Exit the loop if email is sent successfully
            }
            catch (Exception ex)
            {
                string warningMessage =
                    $"Warning: Failed to send email using port {port.Port}. Error: {ex.Message}";
                errorLogs.Add(warningMessage);
                Console.WriteLine(warningMessage);
            }

            if (!isDeliverySuccessful)
            {
                const string errorMessage =
                    "Error: Failed to send email using all available ports.";
                errorLogs.Add(errorMessage);
                Console.WriteLine(errorMessage);
            }
        }

        // try
        // {
        //     using var client = new SmtpClient();
        //     await client.ConnectAsync(
        //         settings["SmtpServer"]!,
        //         int.Parse(settings["SmtpPort"]!),
        //         SecureSocketOptions.SslOnConnect
        //     );
        //     await client.AuthenticateAsync(settings["SenderEmail"]!, settings["SenderPassword"]!);
        //     await client.SendAsync(emailMessage);
        //     await client.DisconnectAsync(true);
        // }
        // catch (Exception ex)
        // {
        //     // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
        //     Console.WriteLine($"Error sending email: {ex.Message}");
        // }
    }
}
