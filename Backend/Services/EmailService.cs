using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace MercBooking.Api.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        if(to == string.Empty || to == null)
            return;
        
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            _configuration["EmailSettings:FromName"],
            _configuration["EmailSettings:FromEmail"]
        ));


        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = body
        }.ToMessageBody();

        //for use
        using var client = new SmtpClient();

        //for testing
        //using var protocolLogger = new ProtocolLogger("smtp-log.txt");
        //using var client = new SmtpClient(protocolLogger);


        var host = _configuration["EmailSettings:Host"];
        var port = _configuration["EmailSettings:Port"];
        var username = _configuration["EmailSettings:Username"];
        var password = _configuration["EmailSettings:Password"];

        //for use
        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(port) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new Exception("Missing email configuration!");
        }

        //for test
        // Console.WriteLine($"Host: {host ?? "MISSING"}");
        // Console.WriteLine($"Port: {port ?? "MISSING"}");
        // Console.WriteLine($"Username: {username ?? "MISSING"}");
        // Console.WriteLine($"Password: {(string.IsNullOrWhiteSpace(password) ? "MISSING" : "LOADED")}");

        await client.ConnectAsync(
            host,
            int.Parse(port),
            SecureSocketOptions.SslOnConnect
        );

        try
        {
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(new SmtpSendRequest(message));
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true);
        }
    }
}
