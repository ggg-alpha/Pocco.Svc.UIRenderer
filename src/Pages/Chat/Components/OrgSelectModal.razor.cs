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

        await Task.CompletedTask;
    }

    public async Task Show(MouseEventArgs e) {
        _hideModal = false;
        await InvokeAsync(StateHasChanged);
    }
    private async Task OnOrgSwitcherOrgSelectedClicked(MouseEventArgs e, string orgId) {
        NavigationManager.NavigateTo($"/chat/{orgId}");

        await Task.CompletedTask;
    }
    private async Task Hide(MouseEventArgs e) {
        _hideModal = true;
        await InvokeAsync(StateHasChanged);
    }
    private async Task OnOrgSearchInputChanged(ChangeEventArgs e) {
        var value = e.Value?.ToString() ?? string.Empty;

        await Task.CompletedTask;
    }

    private async Task OnOrgJoinClicked(MouseEventArgs e) {
        if (ParentPageRef.OrgJoinModalRef is not null) {
            await ParentPageRef.OrgJoinModalRef.Show(e);
        }
    }
    private async Task OnOrgCreateClicked(MouseEventArgs e) {
        if (ParentPageRef.OrgCreateModalRef is not null) {
            await ParentPageRef.OrgCreateModalRef.Show(e);
        }
    }
}
