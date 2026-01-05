using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Pocco.APIClient.Core;
using Pocco.Client.Web.Pages.Chat.Components;
using Pocco.Client.Web.Services;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.Client.Web.Pages.Chat;

public partial class Page : ComponentBase {
    [Parameter] public string? OrgId { get; set; } = string.Empty;

    [Inject] public ILogger<Page> Logger { get; set; } = null!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = null!;

    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    [Inject] public ProtectedLocalStorageProvider LocalStorageProvider { get; set; } = null!;
    [Inject] public APIClient.Core.APIClient ApiClient { get; set; } = null!;
    public Components.OrgSelectModal? OrgSelectModalRef;
    public Components.OrgJoinModal? OrgJoinModalRef;
    public Components.OrgCreateModal? OrgCreateModalRef;
    public Components.OrgSettingsModal? OrgSettingsModalRef;

    public Components.OrgInfoCenter? OrgInfoCenterRef;
    public Components.ChatList? ChatListRef;
    public Components.ChattingArea? ChattingAreaRef;

    public string CurrentOrgName = string.Empty;

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
                NavigationManager.NavigateTo("/hub");
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
                await InvokeAsync(() => NavigationManager.NavigateTo($"/orgs/{evt.Organization.OrganizationId}", forceLoad: true));
            });
            ApiClient.EventHub.GetObservable<ClientEvents.OnOrganizationInfoDeleted>().Subscribe(async (evt) => {
                Logger.LogInformation("Received OnOrganizationInfoDeleted event for Org ID: {OrgId}", evt.OrganizationId);

                // Navigate to the newly created organization's chat page
                await InvokeAsync(() => NavigationManager.NavigateTo($"/chat/direct-messages", forceLoad: false));
            });

            ApiClient.EventHub.GetObservable<ClientEvents.OnOrganizationChatCreated>().Subscribe(async (evt) => {
                Logger.LogInformation("Received OnOrganizationChatCreated event for Org ID: {OrgId}", evt);

                if (ChatListRef is null) {
                    Logger.LogWarning("ChatListRef is null; cannot update chat list.");
                    return;
                }

                await InvokeAsync(async () => await ChatListRef.InitializeAsync());
            });
            ApiClient.EventHub.GetObservable<ClientEvents.OnOrganizationChatUpdated>().Subscribe(async (evt) => {
                Logger.LogInformation("Received OnOrganizationChatUpdated event for Org ID: {OrgId}", evt);

                if (ChatListRef is null) {
                    Logger.LogWarning("ChatListRef is null; cannot update chat list.");
                    return;
                }

                await InvokeAsync(async () => await ChatListRef.InitializeAsync());
            });
            ApiClient.EventHub.GetObservable<ClientEvents.OnOrganizationChatDeleted>().Subscribe(async (evt) => {
                Logger.LogInformation("Received OnOrganizationChatDeleted event for Org ID: {OrgId}", evt);

                if (ChatListRef is null) {
                    Logger.LogWarning("ChatListRef is null; cannot update chat list.");
                    return;
                }

                await InvokeAsync(async () => await ChatListRef.InitializeAsync());
            });

            ApiClient.EventHub.GetObservable<ClientEvents.OnOrganizationSendMessage>().Subscribe(async (evt) => { //? 何故かここにたどり着く前にイベントの購読がキャンセルされる
                Logger.LogInformation("Received OnOrganizationSendMessage event for Org ID: {OrgId}", evt);

                // Get user profile
                try {
                    var user = await ApiClient.GetAccountAsync(new V0AccountGetProfileRequest {
                        UserId = evt.Message.SenderId
                    });

                    // Add new message to dictionary
                    var converted = evt.Message.Content.Replace("\\n", "\n");
                    await JSRuntime.InvokeVoidAsync(
                        "window.MessageContentHelper.createMessage",
                        $"{evt.Message.MessageId}",
                        $"{user.Username}",
                        converted,
                        $"{evt.Message.CreatedAt.ToDateTime().ToString("yyyy/MM/dd HH:mm:ss")}"
                    );
                } catch (Exception ex) {
                    Logger.LogWarning("Error occurred while generating message: {0}", ex.Message);
                }

                await InvokeAsync(StateHasChanged);
            });
            ApiClient.EventHub.GetObservable<ClientEvents.OnOrganizationUpdateMessage>().Subscribe(async (evt) => {
                Logger.LogInformation("Received OnOrganizationUpdateMessage event for Org ID: {OrgId}", evt);

                // Update message in dictionary
            });
            ApiClient.EventHub.GetObservable<ClientEvents.OnOrganizationDeleteMessage>().Subscribe(async (evt) => {
                Logger.LogInformation("Received OnOrganizationDeleteMessage event for Org ID: {OrgId}", evt);

                // Delete message from dictionary
            });

            // Start listening to events
            _ = Task.Run(async () => await ApiClient.EventListener.StartListeningAsync(new ListenRequest {
                UserId = sessionData?.AccountId ?? string.Empty,
                SessionId = sessionData?.SessionId ?? string.Empty
            }));
            // Start session renewal
            _ = Task.Run(async () => await ApiClient.SessionManager.AutoRefreshSessionAsync());

            // Load component datas
            if (OrgSelectModalRef is not null) {
                // Convert to internal data
                var orgInfos = orgs.Select(item => new OrgInfo {
                    Id = item.OrganizationId,
                    Name = item.Name,
                    Description = item.Description,
                }).ToList();

                await OrgSelectModalRef.SetOrganizationsAsync(orgInfos);
            }

            // Load organization info
            if (OrgInfoCenterRef is not null) {
                await OrgInfoCenterRef.GetOrganizationInfo();
            } else {
                Logger.LogWarning("OrgInfoCenterRef is null; cannot load organization info.");
            }
            if (OrgSettingsModalRef is not null) {
                await OrgSettingsModalRef.GetOrganizationInfo();
            } else {
                Logger.LogWarning("OrgSettingsModalRef is null; cannot load organization info.");
            }
            // Load chat list for org
            if (ChatListRef is not null) {
                await ChatListRef.InitializeAsync();
            } else {
                Logger.LogWarning("ChatListRef is null; cannot load chat list.");
            }

            // Load messages
            if (ChattingAreaRef is not null) {
                await ChattingAreaRef.Initialize(true);
            } else {
                Logger.LogWarning("ChattingAreaRef is null; cannot load messages.");
            }

            // State change call
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnChatCategoryToggled(MouseEventArgs e) {
        _expandCategory = !_expandCategory;

        await InvokeAsync(StateHasChanged);
    }
}
