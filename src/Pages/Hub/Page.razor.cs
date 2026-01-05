using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pocco.APIClient.Core;
using Pocco.Client.Web.Services;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.Client.Web.Pages.Hub;

partial class Page : ComponentBase {
    [Inject] public ILogger<Page> Logger { get; set; } = null!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = null!;

    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    [Inject] public ProtectedLocalStorageProvider LocalStorageProvider { get; set; } = null!;
    [Inject] public APIClient.Core.APIClient ApiClient { get; set; } = null!;

    private readonly List<Organization> _orgs = [];

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
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
                        return;
                    }
                } catch (Exception ex) {
                    Logger.LogError(ex, "Failed to verify session data.");
                    NavigationManager.NavigateTo("/login");
                    return;
                }
            } else {
                Logger.LogWarning("No session data found in local storage. Staying on login page.");
                NavigationManager.NavigateTo("/login");
            }

            // Get account profile
            var profile = await ApiClient.GetAccountAsync(new V0AccountGetProfileRequest {
                UserId = sessionData?.AccountId ?? string.Empty
            });

            // Get organizations with account.orgIds
            foreach (var orgId in profile.OrganizationIds) {
                try {
                    var org = await ApiClient.GetOrganizationAsync(new V0BaseRequest {
                        Id = orgId
                    });
                    _orgs.Add(org);
                } catch (Exception ex) {
                    Logger.LogError(ex, "Failed to get organization data for OrgId: {OrgId}", orgId);
                }
            }

            // Save profile and orgs to local storage
            await LocalStorageProvider.SetProfileAsync(profile);
            await LocalStorageProvider.SetOrganizationsAsync(_orgs);

            // Register Events
            ApiClient.EventHub.GetObservable<ClientEvents.OnSessionRefreshed>().Subscribe(async (evt) => {
                Logger.LogInformation("GrpcClientFeeder received OnSessionRefreshed event");
                await LocalStorageProvider.SetSessionDataAsync(evt.Session);
            });
            ApiClient.EventHub.GetObservable<ClientEvents.OnOrganizationInfoCreated>().Subscribe(async (evt) => {
                Logger.LogInformation("Received OnOrganizationCreated event for Org ID: {OrgId}", evt.Organization.OrganizationId);

                // Navigate to the newly created organization's chat page
                await InvokeAsync(() => NavigationManager.NavigateTo($"/orgs/{evt.Organization.OrganizationId}", forceLoad: true));
            });
            ApiClient.EventHub.GetObservable<ClientEvents.OnOrganizationInfoDeleted>().Subscribe(async (evt) => {
                Logger.LogInformation("Received OnOrganizationInfoDeleted event for Org ID: {OrgId}", evt.OrganizationId);

                // Navigate to the newly created organization's chat page
                await InvokeAsync(() => NavigationManager.NavigateTo($"/chat/direct-messages", forceLoad: false));
            });

            ApiClient.EventHub.GetObservable<ClientEvents.OnOrganizationSendMessage>().Subscribe(async (evt) => {
                Logger.LogInformation("Received OnOrganizationSendMessage event for Org ID: {OrgId}", evt);

                // Add icon to org item

                await InvokeAsync(StateHasChanged);
            });

            // Start listening to events
            _ = Task.Run(async () => await ApiClient.EventListener.StartListeningAsync(new ListenRequest {
                UserId = sessionData?.AccountId ?? string.Empty,
                SessionId = sessionData?.SessionId ?? string.Empty
            }));
            // Start session renewal
            _ = Task.Run(async () => await ApiClient.SessionManager.AutoRefreshSessionAsync());

            // State change call
            await InvokeAsync(StateHasChanged);
        }
    }

}
