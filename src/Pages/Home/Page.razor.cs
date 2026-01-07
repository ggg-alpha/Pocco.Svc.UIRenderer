using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Home;

partial class Page : ComponentBase {
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    protected override void OnInitialized() {
        GoToLoginPage();
    }

    private void GoToLoginPage() {
        NavigationManager.NavigateTo("/login");
    }
}
