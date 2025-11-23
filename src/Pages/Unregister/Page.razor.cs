using System.Text.Json;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pocco.Client.Web.Services;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.Client.Web.Pages.Unregister;

partial class Page : ComponentBase
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
                NavigationManager.NavigateTo("/apps/v2/chat");
            }
            else
            {
                Logger.LogWarning("No session data found in local storage. Staying on login page.");
            }

            Console.WriteLine("Chat Page Rendered");
        }

        await HandleUnregister();
    }

    private async Task HandleUnregister()
    {
        try
        {
            // Get session data from local storage
            var sessionData = await LocalStorageProvider.GetSessionDataAsync();
            if (sessionData is null)
            {
                NavigationManager.NavigateTo("/login");
                return;
            }

            Logger.LogInformation("{Data}", JsonSerializer.Serialize(sessionData));

            await ApiClient.DeleteAccountAsync();
            await ApiClient.SessionManager.LogoutAsync();

            await LocalStorageProvider.ClearSessionDataAsync();
            NavigationManager.NavigateTo("/register");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Registration failed: {ex.Message}");
            return;
        }
    }
}
