using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pocco.APIClient.Core;
using Pocco.Client.Web.Services;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class OrgJoinModal : ComponentBase {
    [Parameter] public Page ParentPageRef { get; set; } = null!;
    [Parameter] public ILogger<Page> Logger { get; set; } = null!;
    [Parameter] public NavigationManager NavigationManager { get; set; } = null!;
    [Parameter] public ProtectedLocalStorageProvider LocalStorageProvider { get; set; } = null!;
    [Parameter] public APIClient.Core.APIClient ApiClient { get; set; } = null!;

    private bool _hideJoinModal = true;
    private string _codeInputError = string.Empty;


    protected override async Task OnInitializedAsync() {
        ParentPageRef.OrgJoinModalRef = this;
    }

    public async Task Show(MouseEventArgs e) {
        Logger.LogInformation("Organization create or join opened!");
        _hideJoinModal = false;
        StateHasChanged();
    }
    private async Task Hide(MouseEventArgs e) {
        Logger.LogInformation("Organization create or join closed!");
        _hideJoinModal = true;
        StateHasChanged();
    }

    private async Task OnJoinCodeInputChange(ChangeEventArgs e) {
        var value = e.Value?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(value)) {
            _codeInputError = "招待コードを入力してください。";
        } else {
            _codeInputError = string.Empty;
        }

        Logger.LogInformation("Join code input changed: {JoinCode}", value);
    }

    private async Task OnJoinSubmitClicked() {
        Logger.LogInformation("Join submit clicked!");
    }
}
