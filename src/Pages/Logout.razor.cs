using System.Text.Json;
using Google.Protobuf;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pocco.Client.Web.Clients;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Client.Web.Pages;

public partial class Logout
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ILogger<Login> Logger { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private AuthClient AuthClient { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await HandleLogout();
    }

    private async Task HandleLogout()
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

        Logger.LogInformation("{Data}", sessionDataString);

        try
        {
            var response = await AuthClient.UnauthenticateAsync(sessionData);

            // Clear session data from local storage
            await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "sessionData");
            NavigationManager.NavigateTo("/logout-success");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Logout failed: {ex.Message}");
            NavigationManager.NavigateTo("/apps");
        }
    }

    private void GoToRegisterPage()
    {
        NavigationManager.NavigateTo("/register");
    }
}
