using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using HetznerWhatsApp.Api.Models;

namespace HetznerWhatsApp.Api.Services;

public interface IHetznerService
{
    Task<List<HetznerServer>> GetServersAsync();
    Task<HetznerServer?> GetServerAsync(long serverId);
    Task<HetznerServer?> GetServerByNameAsync(string name);
    Task<bool> PowerOnServerAsync(long serverId);
    Task<bool> PowerOffServerAsync(long serverId);
    Task<bool> RebootServerAsync(long serverId);
    Task<bool> ShutdownServerAsync(long serverId);
}

public class HetznerService : IHetznerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HetznerService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public HetznerService(HttpClient httpClient, IConfiguration configuration, ILogger<HetznerService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        var apiToken = configuration["Hetzner:ApiToken"] 
            ?? throw new InvalidOperationException("Hetzner API token not configured");
        
        _httpClient.BaseAddress = new Uri("https://api.hetzner.cloud/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<HetznerServer>> GetServersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("servers");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<HetznerServersResponse>(content, _jsonOptions);
            
            return result?.Servers ?? new List<HetznerServer>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get servers from Hetzner");
            throw;
        }
    }

    public async Task<HetznerServer?> GetServerAsync(long serverId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"servers/{serverId}");
            if (!response.IsSuccessStatusCode) return null;
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<HetznerServerResponse>(content, _jsonOptions);
            
            return result?.Server;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get server {ServerId} from Hetzner", serverId);
            throw;
        }
    }

    public async Task<HetznerServer?> GetServerByNameAsync(string name)
    {
        var servers = await GetServersAsync();
        return servers.FirstOrDefault(s => 
            s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> PowerOnServerAsync(long serverId)
    {
        return await ExecuteServerActionAsync(serverId, "poweron");
    }

    public async Task<bool> PowerOffServerAsync(long serverId)
    {
        return await ExecuteServerActionAsync(serverId, "poweroff");
    }

    public async Task<bool> RebootServerAsync(long serverId)
    {
        return await ExecuteServerActionAsync(serverId, "reboot");
    }

    public async Task<bool> ShutdownServerAsync(long serverId)
    {
        return await ExecuteServerActionAsync(serverId, "shutdown");
    }

    private async Task<bool> ExecuteServerActionAsync(long serverId, string action)
    {
        try
        {
            var response = await _httpClient.PostAsync(
                $"servers/{serverId}/actions/{action}", 
                new StringContent(""));
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to execute {Action} on server {ServerId}: {StatusCode}", 
                    action, serverId, response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute {Action} on server {ServerId}", action, serverId);
            return false;
        }
    }
}
