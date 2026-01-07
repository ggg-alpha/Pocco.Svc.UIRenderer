using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pocco.APIClient.Core;
using Pocco.Client.Web.Services;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.Client.Web.Pages.JoinOrg;

public partial class Page : ComponentBase {
    [Inject] public ILogger<Page> Logger { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    [Inject] public ProtectedLocalStorageProvider LocalStorageProvider { get; set; } = null!;
    [Inject] public APIClient.Core.APIClient ApiClient { get; set; } = null!;

    private bool _hideJoinModal = true;
    private string _codeInputError = string.Empty;
    private string _joinCode = string.Empty;

    protected override async Task OnInitializedAsync() {
        await Task.CompletedTask;
    }

    public async Task Show(MouseEventArgs e) {
        _hideJoinModal = false;
        await InvokeAsync(StateHasChanged);
    }
    private async Task Hide(MouseEventArgs e) {
        _hideJoinModal = true;
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnJoinCodeInputChange(ChangeEventArgs e) {
        if (string.IsNullOrWhiteSpace(_joinCode)) {
            _codeInputError = "招待コードを入力してください。";
        } else {
            _codeInputError = string.Empty;
        }

        Logger.LogInformation("Join code input changed: {JoinCode}", _joinCode);

        await Task.CompletedTask;
    }

    private async Task OnJoinSubmitClicked() {
        if (string.IsNullOrEmpty(_joinCode)) {
            _codeInputError = "招待コードを入力してください。";
            return;
        }

        _codeInputError = string.Empty;

        try {
            var sessionData = await LocalStorageProvider.GetSessionDataAsync();

            if (sessionData is null) {
                _codeInputError = "セッションデータが存在しません。";
                return;
            }

            var response = await ApiClient.JoinOrganizationMemberAsync(new V0JoinMemberRequest {
                OrganizationId = _joinCode,
                UserId = sessionData.AccountId
            });
        } catch (Exception ex) {
            Logger.LogError(ex, "Failed to join organization with code: {JoinCode}", _joinCode);
        }

        await Task.CompletedTask;
    }
}
