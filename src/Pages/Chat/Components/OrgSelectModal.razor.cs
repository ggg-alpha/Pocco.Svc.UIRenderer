using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pocco.APIClient.Core;
using Pocco.Client.Web.Services;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class OrgSelectModal : ComponentBase {
    [Parameter] public Page ParentPageRef { get; set; } = null!;
    [Parameter] public ILogger<Page> Logger { get; set; } = null!;
    [Parameter] public NavigationManager NavigationManager { get; set; } = null!;
    [Parameter] public ProtectedLocalStorageProvider LocalStorageProvider { get; set; } = null!;
    [Parameter] public APIClient.Core.APIClient ApiClient { get; set; } = null!;

    public bool _hideModal { get; set; } = true;

    protected override async Task OnInitializedAsync() {
        ParentPageRef.OrgSelectModalRef = this;
    }

    public async Task Show(MouseEventArgs e) { // コンポーネント化するときにorgIdをクラスパラメータにする
        Logger.LogInformation("Organization switcher opened!");
        _hideModal = false;
        StateHasChanged();
    }
    private async Task OnOrgSwitcherOrgSelectedClicked(MouseEventArgs e, string orgId) {
        Logger.LogInformation("Organization selected from switcher: {OrgId}", orgId);
        NavigationManager.NavigateTo($"/chat/{orgId}");
    }
    private async Task Hide(MouseEventArgs e) {
        Logger.LogInformation("Organization switcher closed!");
        _hideModal = true;
        StateHasChanged();
    }
    private async Task OnOrgSearchInputChanged(ChangeEventArgs e) {
        var value = e.Value?.ToString() ?? string.Empty;

        Logger.LogInformation("Organization search input changed: {SearchInput}", value);
    }

    private async Task OnOrgJoinClicked(MouseEventArgs e) {
        Logger.LogInformation("Organization join clicked from switcher!");

        if (ParentPageRef.OrgJoinModalRef is not null) {
            Logger.LogInformation("Calling OrgJoinModal.Show()");
            await ParentPageRef.OrgJoinModalRef.Show(e);
        }
    }
    private async Task OnOrgCreateClicked(MouseEventArgs e) {
        Logger.LogInformation("Organization create clicked from switcher!");

        if (ParentPageRef.OrgCreateModalRef is not null) {
            Logger.LogInformation("Calling OrgCreateModal.Show()");
            await ParentPageRef.OrgCreateModalRef.Show(e);
        }
    }
}
