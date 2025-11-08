using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Mvc;
using Pocco.Client.Web.Models;

namespace Pocco.Client.Web.Services;

public class ProtectedLocalStorageProvider(
    [FromServices] ProtectedLocalStorage storage,
    [FromServices] ILogger<ProtectedLocalStorageProvider> logger
)
{
    private readonly ProtectedLocalStorage _storage = storage;
    private const string Key = "sessionData";

    private readonly ILogger<ProtectedLocalStorageProvider> _logger = logger;

    public async Task<SessionData?> GetSessionDataAsync()
    {
        var result = await _storage.GetAsync<SessionData>(Key);
        if (result.Success)
        {
            _logger.LogInformation("SessionData retrieved from ProtectedLocalStorage: {SessionData}", result.Value);
            return result.Value;
        }
        else
        {
            _logger.LogWarning("No SessionData found in ProtectedLocalStorage.");
            return null;
        }
    }

    public async Task SetSessionDataAsync(SessionData data)
    {
        await _storage.SetAsync(Key, data);
        _logger.LogInformation("SessionData saved to ProtectedLocalStorage: {SessionData}", data);
    }


    public async Task ClearSessionDataAsync()
    {
        await _storage.DeleteAsync(Key);
        _logger.LogInformation("SessionData cleared from ProtectedLocalStorage.");
    }

    public async Task SetDeviceIdAsync(Guid deviceId)
    {
        await _storage.SetAsync("deviceId", deviceId);
        _logger.LogInformation("DeviceId saved to ProtectedLocalStorage: {DeviceId}", deviceId);
    }

    public async Task<Guid?> GetDeviceIdAsync()
    {
        var result = await _storage.GetAsync<Guid>("deviceId");
        if (result.Success)
        {
            _logger.LogInformation("DeviceId retrieved from ProtectedLocalStorage: {DeviceId}", result.Value);
            return result.Value;
        }
        else
        {
            _logger.LogWarning("No DeviceId found in ProtectedLocalStorage.");
            return null;
        }
    }

    public async Task ClearDeviceIdAsync()
    {
        await _storage.DeleteAsync("deviceId");
        _logger.LogInformation("DeviceId cleared from ProtectedLocalStorage.");
    }
}
