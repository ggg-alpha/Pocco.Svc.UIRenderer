using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pocco.Client.Web.Services;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.Client.Web.Pages.Logout;

public partial class Page : ComponentBase
{
    [Inject] private APIClient.Core.APIClient ApiClient { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ProtectedLocalStorageProvider LocalStorageProvider { get; set; } = null!;
    [Inject] protected ILogger<Page> Logger { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Verify session
            var sessionData = await LocalStorageProvider.GetSessionDataAsync();
            if (sessionData is not null)
            {
                Logger.LogInformation($"Session data found for user ID: {sessionData.AccountId}");
                await ApiClient.SessionManager.VerifySessionAsync(sessionData);
            }
            else
            {
                Logger.LogWarning("No session data found in local storage. Staying on login page.");
            }
        }

        await HandleLogout();
    }

    private async Task HandleLogout()
    {
        // Get session data from local storage
        var sessionData = await LocalStorageProvider.GetSessionDataAsync();
        if (sessionData is null)
        {
            NavigationManager.NavigateTo("/login");
            return;
        }

        Logger.LogInformation("{Data}", JsonSerializer.Serialize(sessionData));

        try
        {
            var isLoggedOut = await ApiClient.SessionManager.LogoutAsync();
            if (isLoggedOut)
            {
                Logger.LogInformation("Logout successful.");
            }
            else
            {
                Logger.LogWarning("Logout failed on server side. But clearing local session data.");
            }

            await LocalStorageProvider.ClearSessionDataAsync();
            NavigationManager.NavigateTo("/login");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Logout failed: {ex.Message}");
            NavigationManager.NavigateTo("/apps");
        }
    }
}
