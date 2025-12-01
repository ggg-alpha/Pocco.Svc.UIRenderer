using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class OrgSettingsModal : ComponentBase {
    public enum OrgSettingsMode {
        General,
    }

    [Parameter] public Page ParantPage { get; set; } = null!;
    private OrgSettingsMode _orgSettingsMode = OrgSettingsMode.General;

    private bool _hideModal = true;

    protected override void OnInitialized() {
        ParantPage.OrgSettingsModalRef = this;
        base.OnInitialized();
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

    private async Task OnDeleteOrganizationBtnClicked(MouseEventArgs e) {
        try {
            await ParantPage.ApiClient.DeleteOrganizationAsync(new V0BaseRequest {
                Id = ParantPage.OrgId ?? string.Empty,
            });
            await InvokeAsync(StateHasChanged);
        } catch (Exception ex) {
            ParantPage.Logger.LogError(ex, "Failed to delete organization with ID: {OrgId}", ParantPage.OrgId);
        }

    }
}
