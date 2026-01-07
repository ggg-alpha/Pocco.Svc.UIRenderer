using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Mvc;
using Pocco.APIClient.Core;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.Client.Web.Services;

public class ProtectedLocalStorageProvider(
    [FromServices] ProtectedLocalStorage storage,
    [FromServices] ILogger<ProtectedLocalStorageProvider> logger
) {
    private readonly ProtectedLocalStorage _storage = storage;
    private const string Key = "sessionData";

    private readonly ILogger<ProtectedLocalStorageProvider> _logger = logger;

    public async Task<SessionData?> GetSessionDataAsync() {
        var result = await _storage.GetAsync<SessionData>(Key);
        if (result.Success) {
            _logger.LogInformation("SessionData retrieved from ProtectedLocalStorage: {SessionData}", result.Value);
            return result.Value;
        } else {
            _logger.LogWarning("No SessionData found in ProtectedLocalStorage.");
            return null;
        }
    }

    public async Task SetSessionDataAsync(SessionData data) {
        await _storage.SetAsync(Key, data);
        _logger.LogInformation("SessionData saved to ProtectedLocalStorage: {SessionData}", data);
    }


    public async Task ClearSessionDataAsync() {
        await _storage.DeleteAsync(Key);
        _logger.LogInformation("SessionData cleared from ProtectedLocalStorage.");
    }

    public async Task SetDeviceIdAsync(Guid deviceId) {
        await _storage.SetAsync("deviceId", deviceId);
        _logger.LogInformation("DeviceId saved to ProtectedLocalStorage: {DeviceId}", deviceId);
    }

    public async Task SetProfileAsync(V0BaseAccount profile) {
        await _storage.SetAsync("profile", profile);
        _logger.LogInformation("Profile saved to ProtectedLocalStorage: {Profile}", profile);
    }

    public async Task<V0BaseAccount?> GetProfileAsync() {
        var result = await _storage.GetAsync<V0BaseAccount>("profile");
        if (result.Success) {
            _logger.LogInformation("Profile retrieved from ProtectedLocalStorage: {Profile}", result.Value);
            return result.Value;
        } else {
            _logger.LogWarning("No Profile found in ProtectedLocalStorage.");
            return null;
        }
    }

    public async Task<bool> ClearProfileAsync() {
        await _storage.DeleteAsync("profile");
        _logger.LogInformation("Profile cleared from ProtectedLocalStorage.");
        return true;
    }

    public async Task SetOrganizationsAsync(List<Organization> orgs) {
        await _storage.SetAsync("organizations", orgs);
        _logger.LogInformation("Organizations saved to ProtectedLocalStorage: {Organizations}", orgs);
    }
    public async Task<List<Organization>?> GetOrganizationsAsync() {
        var result = await _storage.GetAsync<List<Organization>>("organizations");
        if (result.Success) {
            _logger.LogInformation("Organizations retrieved from ProtectedLocalStorage: {Organizations}", result.Value);
            return result.Value;
        } else {
            _logger.LogWarning("No Organizations found in ProtectedLocalStorage.");
            return null;
        }
    }
    public async Task<bool> ClearOrganizationsAsync() {
        await _storage.DeleteAsync("organizations");
        _logger.LogInformation("Organizations cleared from ProtectedLocalStorage.");
        return true;
    }
}
