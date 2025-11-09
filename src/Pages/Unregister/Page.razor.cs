using System.Text.Json;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pocco.Client.Web.Services;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Client.Web.Pages.Unregister;

partial class Page : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private ILogger<Page> Logger { get; set; } = null!;
    [Inject] private GrpcClientFeederProvider ClientFeederProvider { get; set; } = null!;
    [Inject] private ProtectedLocalStorageProvider LocalStorageProvider { get; set; } = null!;
    private GrpcClientFeeder? _clientFeeder;

    private string email = string.Empty;

    private string password = string.Empty;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var scopedServiceId = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "scopedServiceId");
            var id = Guid.TryParse(scopedServiceId, out var guid) ? guid : Guid.NewGuid();
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "scopedServiceId", id.ToString());

            _clientFeeder = ClientFeederProvider.GetOrCreate(id, () => new GrpcClientFeeder(id, LocalStorageProvider, Logger));

            await HandleUnregister();
        }
    }

    private async Task HandleUnregister()
    {
        if (_clientFeeder == null)
        {
            Logger.LogError("GrpcClientFeeder is not initialized.");
            return;
        }

        try
        {
            // Get session data from local storage
            var sessionDataString = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "sessionData"); // row string
            if (string.IsNullOrWhiteSpace(sessionDataString))
            {
                NavigationManager.NavigateTo("/login");
                return;
            }
            // Convert session data string to V0ApiSessionData
            var sessionData = JsonSerializer.Deserialize<V0ApiSessionData>(sessionDataString);
            if (sessionData == null)
            {
                NavigationManager.NavigateTo("/apps");
                return;
            }

            await _clientFeeder.UnregisterAccountAsync(sessionData);

            NavigationManager.NavigateTo("/login");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Registration failed: {ex.Message}");
            return;
        }
    }
}
