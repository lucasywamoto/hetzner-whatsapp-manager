using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace HetznerWhatsApp.Api.Services;

public interface IWhatsAppService
{
    Task SendMessageAsync(string to, string message);
}

public class TwilioWhatsAppService : IWhatsAppService
{
    private readonly string _fromNumber;
    private readonly ILogger<TwilioWhatsAppService> _logger;

    public TwilioWhatsAppService(IConfiguration configuration, ILogger<TwilioWhatsAppService> logger)
    {
        _logger = logger;
        
        var accountSid = configuration["Twilio:AccountSid"] 
            ?? throw new InvalidOperationException("Twilio AccountSid not configured");
        var authToken = configuration["Twilio:AuthToken"] 
            ?? throw new InvalidOperationException("Twilio AuthToken not configured");
        _fromNumber = configuration["Twilio:WhatsAppNumber"] 
            ?? throw new InvalidOperationException("Twilio WhatsApp number not configured");

        TwilioClient.Init(accountSid, authToken);
    }

    public async Task SendMessageAsync(string to, string message)
    {
        try
        {
            var toNumber = to.StartsWith("whatsapp:") ? to : $"whatsapp:{to}";
            var fromNumber = _fromNumber.StartsWith("whatsapp:") ? _fromNumber : $"whatsapp:{_fromNumber}";

            await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(fromNumber),
                to: new PhoneNumber(toNumber)
            );

            _logger.LogInformation("WhatsApp message sent to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp message to {To}", to);
            throw;
        }
    }
}
