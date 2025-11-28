using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pocco.APIClient.Core;
using Pocco.Client.Web.Services;

namespace Pocco.Client.Web.Pages.Chat;

public partial class Page : ComponentBase {
    [Parameter] public string? OrgId { get; set; } = string.Empty;

    [Inject] public ILogger<Page> Logger { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    [Inject] public ProtectedLocalStorageProvider LocalStorageProvider { get; set; } = null!;
    [Inject] public APIClient.Core.APIClient ApiClient { get; set; } = null!;
    public Components.OrgSelectModal? OrgSelectModalRef;
    public Components.OrgJoinModal? OrgJoinModalRef;
    public Components.OrgCreateModal? OrgCreateModalRef;

    private bool _expandCategory = true;

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            if (string.IsNullOrWhiteSpace(OrgId)) {
                Logger.LogInformation("No organization ID provided in URL. Redirecting to direct messages page.");
                NavigationManager.NavigateTo("/chat/direct-messages");
            }

            ApiClient.EventHub.GetObservable<ClientEvents.OnClientLoggedIn>().Subscribe(async (evt) => {
                Logger.LogInformation("GrpcClientFeeder received OnClientLoggedIn event");
                await LocalStorageProvider.SetSessionDataAsync(evt.Session);
            });

            // Verify session
            var sessionData = await LocalStorageProvider.GetSessionDataAsync();
            if (sessionData is not null) {
                Logger.LogInformation($"Session data found for user ID: {sessionData.AccountId}");
                try {
                    var result = await ApiClient.SessionManager.VerifySessionAsync(sessionData);

                    if (result is false) {
                        Logger.LogWarning("Session verification failed. Redirecting to login page.");
                        NavigationManager.NavigateTo("/login");
                    }
                } catch (Exception ex) {
                    Logger.LogError(ex, "Failed to verify session data.");
                    NavigationManager.NavigateTo("/login");
                }
            } else {
                Logger.LogWarning("No session data found in local storage. Staying on login page.");
                NavigationManager.NavigateTo("/login");
            }
        }
    }

    private async Task OnChatCategoryToggled(MouseEventArgs e) {
        Logger.LogInformation("Chat category toggled!");

        _expandCategory = !_expandCategory;

        StateHasChanged();
    }
}
