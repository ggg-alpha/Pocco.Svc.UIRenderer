using System.Text.Json;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pocco.Client.Web.Services;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Client.Web.Pages.Unregister;

partial class Page : ComponentBase
{
    private GrpcClientFeeder Service { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private GrpcClientFeederProvider ServiceProvider { get; set; } = null!;
    [Inject] private ProtectedLocalStorageProvider LocalStorageProvider { get; set; } = null!;
    [Inject] protected ILogger<Page> Logger { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Get id from session storage (if exists)
            var id = await LocalStorageProvider.GetDeviceIdAsync().ConfigureAwait(false);
            if (id is Guid guid)
            {
                Service = ServiceProvider.GetOrCreate(guid, () => new GrpcClientFeeder(guid, LocalStorageProvider, Logger));
                if (Service is not null)
                {
                    Logger.LogInformation($"Retrieved existing scoped service ID from session storage: {Service.Id}");

                    Service.IncrementConnectionCount();
                }
            }
            else
            {
                Logger.LogInformation("No existing scoped service ID found in session storage. Creating new one.");

                var newId = Guid.NewGuid();
                Service = ServiceProvider.GetOrCreate(newId, () => new GrpcClientFeeder(newId, LocalStorageProvider, Logger));
                Service.IncrementConnectionCount();

                await LocalStorageProvider.SetDeviceIdAsync(newId);

                Logger.LogInformation($"Created new scoped service ID: {Service.Id}");
            }

            if (Service is null)
            {
                Logger.LogError("GrpcClientFeeder service is null. Cannot verify session.");
                // NavigationManager.NavigateTo("/error");
                return;
            }

            // Verify session
            var sessionData = await LocalStorageProvider.GetSessionDataAsync();
            if (sessionData is not null)
            {
                Logger.LogInformation($"Session data found for user ID: {sessionData.AccountId}");
                await Service.ApiClient.SessionManager.VerifySessionAsync(sessionData);
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

            await Service.ApiClient.DeleteAccountAsync();
            await Service.ApiClient.SessionManager.LogoutAsync();

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
