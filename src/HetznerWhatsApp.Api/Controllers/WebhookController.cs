using HetznerWhatsApp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HetznerWhatsApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly ICommandHandler _commandHandler;
    private readonly IWhatsAppService _whatsAppService;
    private readonly ILogger<WebhookController> _logger;
    private readonly IConfiguration _configuration;

    public WebhookController(
        ICommandHandler commandHandler,
        IWhatsAppService whatsAppService,
        ILogger<WebhookController> logger,
        IConfiguration configuration)
    {
        _commandHandler = commandHandler;
        _whatsAppService = whatsAppService;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("twilio")]
    public async Task<IActionResult> TwilioWebhook([FromForm] TwilioWebhookRequest request)
    {
        _logger.LogInformation("Received WhatsApp message from {From}: {Body}", request.From, request.Body);

        var allowedNumbers = _configuration.GetSection("AllowedPhoneNumbers").Get<string[]>() ?? Array.Empty<string>();
        var fromNumber = request.From?.Replace("whatsapp:", "") ?? "";
        
        if (allowedNumbers.Length > 0 && !allowedNumbers.Contains(fromNumber))
        {
            _logger.LogWarning("Unauthorized access attempt from {From}", request.From);
            return Ok();
        }

        try
        {
            var response = await _commandHandler.HandleCommandAsync(request.Body ?? "help");
            await _whatsAppService.SendMessageAsync(request.From!, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            await _whatsAppService.SendMessageAsync(request.From!, "An error occurred. Please try again later.");
        }

        return Ok();
    }

    [HttpGet("twilio")]
    public IActionResult TwilioVerify()
    {
        return Ok("Webhook is active");
    }
}

public class TwilioWebhookRequest
{
    public string? MessageSid { get; set; }
    public string? AccountSid { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }
    public string? Body { get; set; }
}
