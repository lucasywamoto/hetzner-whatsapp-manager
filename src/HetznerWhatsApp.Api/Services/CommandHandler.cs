using System.Text;
using HetznerWhatsApp.Api.Models;

namespace HetznerWhatsApp.Api.Services;

public interface ICommandHandler
{
    Task<string> HandleCommandAsync(string command);
}

public class CommandHandler : ICommandHandler
{
    private readonly IHetznerService _hetznerService;
    private readonly ILogger<CommandHandler> _logger;

    public CommandHandler(IHetznerService hetznerService, ILogger<CommandHandler> logger)
    {
        _hetznerService = hetznerService;
        _logger = logger;
    }

    public async Task<string> HandleCommandAsync(string command)
    {
        var parts = command.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length == 0)
            return GetHelpMessage();

        return parts[0] switch
        {
            "help" => GetHelpMessage(),
            "list" or "servers" => await HandleListServersAsync(),
            "status" => await HandleStatusAsync(parts),
            "start" or "poweron" => await HandlePowerOnAsync(parts),
            "stop" or "poweroff" => await HandlePowerOffAsync(parts),
            "reboot" => await HandleRebootAsync(parts),
            "shutdown" => await HandleShutdownAsync(parts),
            _ => $"Unknown command: {parts[0]}\n\n{GetHelpMessage()}"
        };
    }

    private string GetHelpMessage()
    {
        return """
            *Hetzner Cloud Manager*
            
            Available commands:
            
            *list* - List all servers
            *status <name|id>* - Get server status
            *start <name|id>* - Power on server
            *stop <name|id>* - Force power off
            *shutdown <name|id>* - Graceful shutdown
            *reboot <name|id>* - Reboot server
            *help* - Show this message
            """;
    }

    private async Task<string> HandleListServersAsync()
    {
        try
        {
            var servers = await _hetznerService.GetServersAsync();
            
            if (servers.Count == 0)
                return "No servers found.";

            var sb = new StringBuilder("*Your Servers:*\n\n");
            
            foreach (var server in servers)
            {
                var statusEmoji = server.Status switch
                {
                    "running" => "🟢",
                    "off" => "🔴",
                    "starting" => "🟡",
                    "stopping" => "🟡",
                    _ => "⚪"
                };
                
                sb.AppendLine($"{statusEmoji} *{server.Name}* (ID: {server.Id})");
                sb.AppendLine($"   Status: {server.Status}");
                sb.AppendLine($"   IP: {server.PublicNet?.Ipv4?.Ip ?? "N/A"}");
                sb.AppendLine($"   Type: {server.ServerType?.Name ?? "N/A"}");
                sb.AppendLine($"   Location: {server.Datacenter?.Location?.City ?? "N/A"}");
                sb.AppendLine();
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list servers");
            return "Failed to retrieve servers. Please try again later.";
        }
    }

    private async Task<string> HandleStatusAsync(string[] parts)
    {
        if (parts.Length < 2)
            return "Usage: status <server-name or id>";

        var server = await FindServerAsync(parts[1]);
        if (server == null)
            return $"Server '{parts[1]}' not found.";

        return FormatServerDetails(server);
    }

    private async Task<string> HandlePowerOnAsync(string[] parts)
    {
        if (parts.Length < 2)
            return "Usage: start <server-name or id>";

        var server = await FindServerAsync(parts[1]);
        if (server == null)
            return $"Server '{parts[1]}' not found.";

        if (server.Status == "running")
            return $"Server *{server.Name}* is already running.";

        var success = await _hetznerService.PowerOnServerAsync(server.Id);
        return success 
            ? $"Starting server *{server.Name}*... This may take a moment."
            : $"Failed to start server *{server.Name}*. Please try again.";
    }

    private async Task<string> HandlePowerOffAsync(string[] parts)
    {
        if (parts.Length < 2)
            return "Usage: stop <server-name or id>";

        var server = await FindServerAsync(parts[1]);
        if (server == null)
            return $"Server '{parts[1]}' not found.";

        if (server.Status == "off")
            return $"Server *{server.Name}* is already off.";

        var success = await _hetznerService.PowerOffServerAsync(server.Id);
        return success 
            ? $"Forcing power off for *{server.Name}*..."
            : $"Failed to stop server *{server.Name}*. Please try again.";
    }

    private async Task<string> HandleShutdownAsync(string[] parts)
    {
        if (parts.Length < 2)
            return "Usage: shutdown <server-name or id>";

        var server = await FindServerAsync(parts[1]);
        if (server == null)
            return $"Server '{parts[1]}' not found.";

        if (server.Status == "off")
            return $"Server *{server.Name}* is already off.";

        var success = await _hetznerService.ShutdownServerAsync(server.Id);
        return success 
            ? $"Gracefully shutting down *{server.Name}*..."
            : $"Failed to shutdown server *{server.Name}*. Please try again.";
    }

    private async Task<string> HandleRebootAsync(string[] parts)
    {
        if (parts.Length < 2)
            return "Usage: reboot <server-name or id>";

        var server = await FindServerAsync(parts[1]);
        if (server == null)
            return $"Server '{parts[1]}' not found.";

        var success = await _hetznerService.RebootServerAsync(server.Id);
        return success 
            ? $"Rebooting *{server.Name}*..."
            : $"Failed to reboot server *{server.Name}*. Please try again.";
    }

    private async Task<HetznerServer?> FindServerAsync(string identifier)
    {
        if (long.TryParse(identifier, out var serverId))
        {
            return await _hetznerService.GetServerAsync(serverId);
        }
        
        return await _hetznerService.GetServerByNameAsync(identifier);
    }

    private string FormatServerDetails(HetznerServer server)
    {
        var statusEmoji = server.Status switch
        {
            "running" => "🟢",
            "off" => "🔴",
            "starting" => "🟡",
            "stopping" => "🟡",
            _ => "⚪"
        };

        return $"""
            {statusEmoji} *{server.Name}*
            
            *Status:* {server.Status}
            *ID:* {server.Id}
            *IPv4:* {server.PublicNet?.Ipv4?.Ip ?? "N/A"}
            *IPv6:* {server.PublicNet?.Ipv6?.Ip ?? "N/A"}
            *Type:* {server.ServerType?.Name ?? "N/A"}
            *Specs:* {server.ServerType?.Cores ?? 0} vCPU, {server.ServerType?.Memory ?? 0}GB RAM, {server.ServerType?.Disk ?? 0}GB Disk
            *Location:* {server.Datacenter?.Location?.City ?? "N/A"}, {server.Datacenter?.Location?.Country ?? "N/A"}
            """;
    }
}
