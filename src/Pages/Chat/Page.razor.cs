using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pocco.APIClient.Core;
using Pocco.Client.Web.Services;
using Pocco.Libs.Protobufs.CoreAPI.Services;

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
    public Components.OrgSettingsModal? OrgSettingsModalRef;

    private bool _expandCategory = true;

    protected override async Task OnInitializedAsync() {
        Logger.LogInformation("Chat Page Initialized with OrgId: {OrgId}", OrgId);

        await Task.CompletedTask;
    }

    protected override async Task OnParametersSetAsync() {
        Logger.LogInformation("Chat Page Parameters Set. Current OrgId: {OrgId}", OrgId);

        await Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            if (string.IsNullOrWhiteSpace(OrgId)) {
                Logger.LogInformation("No organization ID provided in URL. Redirecting to direct messages page.");
                NavigationManager.NavigateTo("/chat/direct-messages"); // TODO: ダイレクトメッセージページ（別コンポーネント）に変更
                return;
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

            // Get account profile
            var profile = await ApiClient.GetAccountAsync(new V0AccountGetProfileRequest {
                UserId = sessionData?.AccountId ?? string.Empty
            });

            // Get organizations with account.orgIds
            var orgs = new List<Organization>();
            foreach (var orgId in profile.OrganizationIds) {
                try {
                    var org = await ApiClient.GetOrganizationAsync(new V0BaseRequest {
                        Id = orgId
                    });
                    orgs.Add(org);
                } catch (Exception ex) {
                    Logger.LogError(ex, "Failed to get organization data for OrgId: {OrgId}", orgId);
                }
            }

            // Save profile and orgs to local storage
            await LocalStorageProvider.SetProfileAsync(profile);
            await LocalStorageProvider.SetOrganizationsAsync(orgs);

            // Register Events
            ApiClient.EventHub.GetObservable<ClientEvents.OnSessionRefreshed>().Subscribe(async (evt) => {
                Logger.LogInformation("GrpcClientFeeder received OnSessionRefreshed event");
                await LocalStorageProvider.SetSessionDataAsync(evt.Session);
            });
            ApiClient.EventHub.GetObservable<ClientEvents.OnOrganizationInfoCreated>().Subscribe(async (evt) => {
                Logger.LogInformation("Received OnOrganizationCreated event for Org ID: {OrgId}", evt.Organization.OrganizationId);

                // Navigate to the newly created organization's chat page
                await InvokeAsync(() => NavigationManager.NavigateTo($"/chat/{evt.Organization.OrganizationId}", forceLoad: false));
            });
            ApiClient.EventHub.GetObservable<ClientEvents.OnOrganizationInfoDeleted>().Subscribe(async (evt) => {
                Logger.LogInformation("Received OnOrganizationInfoDeleted event for Org ID: {OrgId}", evt.OrganizationId);

                // Navigate to the newly created organization's chat page
                await InvokeAsync(() => NavigationManager.NavigateTo($"/chat/direct-messages", forceLoad: false));
            });

            // Start listening to events
            await ApiClient.EventListener.StartListeningAsync(new ListenRequest {
                UserId = sessionData?.AccountId ?? string.Empty,
                SessionId = sessionData?.SessionId ?? string.Empty
            });
            // Start session renewal
            await ApiClient.SessionManager.AutoRefreshSessionAsync();
        }
    }

    private async Task OnChatCategoryToggled(MouseEventArgs e) {
        _expandCategory = !_expandCategory;

        await InvokeAsync(StateHasChanged);
    }
}
