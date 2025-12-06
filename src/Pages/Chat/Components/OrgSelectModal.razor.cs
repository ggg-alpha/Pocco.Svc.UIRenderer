using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pocco.APIClient.Core;
using Pocco.Client.Web.Services;

namespace Pocco.Client.Web.Pages.Chat.Components;

public class OrgInfo {
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public partial class OrgSelectModal : ComponentBase {
    [Parameter] public Page ParentPageRef { get; set; } = null!;
    [Parameter] public ILogger<Page> Logger { get; set; } = null!;
    [Parameter] public NavigationManager NavigationManager { get; set; } = null!;
    [Parameter] public ProtectedLocalStorageProvider LocalStorageProvider { get; set; } = null!;
    [Parameter] public APIClient.Core.APIClient ApiClient { get; set; } = null!;

    // TODO: パラメータ化する
    public List<OrgInfo> Orgs { get; set; } = new List<OrgInfo> {
        new OrgInfo {
            Id = "org-1",
            Name = "Organization One",
            Description = "This is the first organization.",
        },
        new OrgInfo {
            Id = "org-2",
            Name = "Organization Two",
            Description = "This is the second organization.",
        },
        new OrgInfo {
            Id = "org-3",
            Name = "Organization Three",
            Description = "This is the third organization.",
        },
    };

    public bool _hideModal { get; set; } = true;

    protected override async Task OnInitializedAsync() {
        ParentPageRef.OrgSelectModalRef = this;

        // 組織リストを名前でソート
        Orgs = Orgs.OrderBy(o => o.Name).ToList();

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
