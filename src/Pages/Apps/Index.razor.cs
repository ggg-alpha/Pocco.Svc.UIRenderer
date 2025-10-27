using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Apps;

partial class Index : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    protected override void OnInitialized()
    {
        GoToLoginPage();
    }

    private void GoToLoginPage()
    {
        NavigationManager.NavigateTo("/apps/chat");
    }
}
