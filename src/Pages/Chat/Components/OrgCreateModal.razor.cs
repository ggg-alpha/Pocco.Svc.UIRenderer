using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pocco.APIClient.Core;
using Pocco.Client.Web.Services;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class OrgCreateModal : ComponentBase {
    [Parameter] public Page ParentPageRef { get; set; } = null!;

    [Parameter] public ILogger<Page> Logger { get; set; } = null!;
    [Parameter] public NavigationManager NavigationManager { get; set; } = null!;
    [Parameter] public ProtectedLocalStorageProvider LocalStorageProvider { get; set; } = null!;
    [Parameter] public APIClient.Core.APIClient ApiClient { get; set; } = null!;

    private bool _hideCreateModal = true;
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
        ParentPageRef.OrgCreateModalRef = this;
    }

    public async Task Show(MouseEventArgs e) {
        Logger.LogInformation("Organization create modal opened!");
        _hideCreateModal = false;
        StateHasChanged();
    }
    private async Task Hide(MouseEventArgs e) {
        Logger.LogInformation("Organization create modal closed!");
        _hideCreateModal = true;
        StateHasChanged();
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
            var orgId = await ApiClient.CreateOrganizationAsync(new V0CreateOrganizationRequest {
                Base = new V0CreateXRequest {
                    Name = _orgName
                },
                Description = _orgDesc
            });

            Logger.LogInformation("Organization created successfully with ID: {OrgId}", orgId);

            // Navigate to the newly created organization's chat page
            NavigationManager.NavigateTo($"/chat/{orgId}");
        } catch (Exception ex) {
            Logger.LogError(ex, "Failed to create organization.");
        }

        await Task.CompletedTask;
    }
}
