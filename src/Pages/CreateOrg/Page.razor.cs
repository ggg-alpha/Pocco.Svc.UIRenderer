using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pocco.APIClient.Core;
using Pocco.Client.Web.Services;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.Client.Web.Pages.CreateOrg;

public partial class Page : ComponentBase {
    [Inject] public APIClient.Core.APIClient ApiClient { get; set; } = null!;
    [Inject] public ILogger<Page> Logger { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    [Inject] public ProtectedLocalStorageProvider LocalStorageProvider { get; set; } = null!;

    private const string OrgNameLengthErrorMessage = "組織名は30文字以内で入力してください。";
    private const string OrgDescOverMaxLengthErrorMessage = "組織説明は200文字以内で入力してください。";
    private string _orgName = string.Empty;
    private string _orgDesc = string.Empty;
    private bool _orgNameInvalid = false;
    private bool _orgDescInvalid = false;
    private bool _canSubmit => !_orgNameInvalid && !_orgDescInvalid;
    private string _orgNameErrorMessage = string.Empty;
    private string _orgDescErrorMessage = string.Empty;


    protected override async Task OnInitializedAsync() {
        await Task.CompletedTask;
    }
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

    private void OnOrgNameChanged(ChangeEventArgs e) {
        _orgName = e.Value?.ToString() ?? string.Empty;
        _orgNameInvalid = string.IsNullOrWhiteSpace(_orgName);

        if (_orgNameInvalid) {
            _orgNameErrorMessage = "組織名は必須項目です。";
        } else if (_orgName.Length > 30) {
            _orgNameInvalid = true;
            _orgNameErrorMessage = OrgNameLengthErrorMessage;
        } else {
            _orgNameErrorMessage = string.Empty;
        }

        StateHasChanged();
    }

    private void OnOrgDescChanged(ChangeEventArgs e) {
        _orgDesc = e.Value?.ToString() ?? string.Empty;
        _orgDescInvalid = _orgDesc.Length > 200;

        if (_orgDescInvalid) {
            _orgDescErrorMessage = OrgDescOverMaxLengthErrorMessage;
        } else {
            _orgDescErrorMessage = string.Empty;
        }
        StateHasChanged();
    }

    private async Task SubmitCreateOrgAsync() {
        if (!_canSubmit) {
            Console.WriteLine("Cannot submit due to validation errors.");
            return;
        }

        try {
            var reply = await ApiClient.CreateOrganizationAsync(new V0CreateOrganizationRequest {
                Base = new V0CreateXRequest {
                    Name = _orgName
                },
                Description = _orgDesc
            });

            Logger.LogInformation("Organization creation requested with EventId: {EventId}", reply.EventId);

            await InvokeAsync(StateHasChanged);

            // Log the api client is listening now
            Logger.LogInformation("The ApiClient is now listening these events: {Filter}", ApiClient.EventListener.CurrentListeningEvents);
        } catch (Exception ex) {
            _orgNameErrorMessage = "組織名は必須項目です。";
            _orgDescErrorMessage = "組織説明は必須項目です。";

            Logger.LogError(ex, "Failed to create organization.");
        }

        await Task.CompletedTask;
    }
}
