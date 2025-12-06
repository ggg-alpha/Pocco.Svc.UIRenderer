using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class OrgSettingsModal : ComponentBase {
    public enum OrgSettingsMode {
        General,
        Roles,
        Chats,
        Others,
    }

    [Parameter] public Page ParentPage { get; set; } = null!;
    private OrgSettingsMode _orgSettingsMode = OrgSettingsMode.General;

    private string _currentOrgName = string.Empty;
    private string _currentOrgDescription = string.Empty;
    private bool _canSaveChanges => IsGeneralSettingsChanged();
    private string OrganizationName { get; set; } = string.Empty;
    private string OrganizationDescription { get; set; } = string.Empty;

    private bool _hideModal = true;

    protected override async Task OnInitializedAsync() {
        ParentPage.OrgSettingsModalRef = this;

        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync() {
        // if (ParentPage.ApiClient is not null && !string.IsNullOrWhiteSpace(ParentPage.OrgId)) {
        //     try {
        //         var orgInfo = await ParentPage.ApiClient.GetOrganizationAsync(new V0BaseRequest {
        //             Id = ParentPage.OrgId,
        //         });

        //         _currentOrgName = orgInfo.Name;
        //         _currentOrgDescription = orgInfo.Description;

        //         OrganizationName = _currentOrgName;
        //         OrganizationDescription = _currentOrgDescription;
        //     } catch (Exception ex) {
        //         ParentPage.Logger.LogError(ex, "Failed to fetch organization info for Org ID: {OrgId}", ParentPage.OrgId);
        //     }
        // }

        await base.OnParametersSetAsync();
    }

    public async Task Show() {
        _hideModal = false;
        await InvokeAsync(StateHasChanged);
    }
    private async Task Hide(MouseEventArgs e) {
        _hideModal = true;
        await InvokeAsync(StateHasChanged);
    }

    private async Task ChangeMode(OrgSettingsMode mode) {
        if (_orgSettingsMode == mode) {
            return;
        }

        _orgSettingsMode = mode;
        await InvokeAsync(StateHasChanged);
    }

    public async Task GetOrganizationInfo() {
        if (ParentPage.ApiClient is not null && !string.IsNullOrWhiteSpace(ParentPage.OrgId)) {
            try {
                var orgInfo = await ParentPage.ApiClient.GetOrganizationAsync(new V0BaseRequest {
                    Id = ParentPage.OrgId,
                });

                _currentOrgName = orgInfo.Name;
                _currentOrgDescription = orgInfo.Description;

                OrganizationName = _currentOrgName;
                OrganizationDescription = _currentOrgDescription;

                ParentPage.Logger.LogInformation("Fetched organization info for Org ID: {OrgId}, Name: {OrgName}", ParentPage.OrgId, OrganizationName);
            } catch (Exception ex) {
                ParentPage.Logger.LogError(ex, "Failed to fetch organization info for Org ID: {OrgId}", ParentPage.OrgId);
            }

            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnUpdateOrganizationBtnClicked(MouseEventArgs e) {
        try {
            await ParentPage.ApiClient.UpdateOrganizationNameAsync(new V0UpdateOrganizationRequest {
                OrganizationId = ParentPage.OrgId ?? string.Empty,
                Name = OrganizationName,
                Description = OrganizationDescription,
            });

            // Update current values
            _currentOrgName = OrganizationName;
            _currentOrgDescription = OrganizationDescription;
            await InvokeAsync(StateHasChanged);
        } catch (Exception ex) {
            ParentPage.Logger.LogError(ex, "Failed to update organization with ID: {OrgId}", ParentPage.OrgId);
        }
    }

    private async Task OnDeleteOrganizationBtnClicked(MouseEventArgs e) {
        try {
            await ParentPage.ApiClient.DeleteOrganizationAsync(new V0BaseRequest {
                Id = ParentPage.OrgId ?? string.Empty,
            });
            await InvokeAsync(StateHasChanged);
        } catch (Exception ex) {
            ParentPage.Logger.LogError(ex, "Failed to delete organization with ID: {OrgId}", ParentPage.OrgId);
        }
    }

    private async Task OnOrganizationNameChanged(ChangeEventArgs e) {
        OrganizationName = e.Value?.ToString() ?? string.Empty;

        await InvokeAsync(StateHasChanged);
    }

    private async Task OnOrganizationDescriptionChanged(ChangeEventArgs e) {
        OrganizationDescription = e.Value?.ToString() ?? string.Empty;

        await InvokeAsync(StateHasChanged);
    }

    private bool IsGeneralSettingsChanged() {
        // Required field check
        if (string.IsNullOrEmpty(OrganizationName)) {
            return false;
        }

        // Check if any changes were made
        return OrganizationName != _currentOrgName || OrganizationDescription != _currentOrgDescription;
    }
}
