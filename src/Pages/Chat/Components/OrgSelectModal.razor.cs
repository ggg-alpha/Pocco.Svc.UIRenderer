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
    [Inject] private IConfiguration _configuration { get; set; } = null!;
    private string _cdnAddress { get; set; } = string.Empty;

    // TODO: パラメータ化する
    public List<OrgInfo> Orgs { get; set; } = new List<OrgInfo>();

    public bool _hideModal { get; set; } = true;

    protected override async Task OnInitializedAsync() {
        ParentPageRef.OrgSelectModalRef = this;
        _cdnAddress = _configuration["CDN_ADDRESS"] ?? "http://localhost:5197";

        // 組織リストを名前でソート
        Orgs = Orgs.OrderBy(o => o.Name).ToList();

        await Task.CompletedTask;
    }

    public async Task SetOrganizationsAsync(List<OrgInfo> orgs) {
        Orgs = orgs;
        await InvokeAsync(StateHasChanged);
    }

    public async Task Show(MouseEventArgs e) {
        _hideModal = false;
        await InvokeAsync(StateHasChanged);
    }
    private async Task OnOrgSwitcherOrgSelectedClicked(MouseEventArgs e, string orgId) {
        Console.WriteLine("Clicked!!");
        _hideModal = true;
        await InvokeAsync(StateHasChanged);

        NavigationManager.NavigateTo($"/orgs/{orgId}", forceLoad: true);

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
        // if (ParentPageRef.OrgCreateModalRef is not null) {
        //     await ParentPageRef.OrgCreateModalRef.Show(e);
        // }

        NavigationManager.NavigateTo("/orgs/new", forceLoad: true);
    }
}
