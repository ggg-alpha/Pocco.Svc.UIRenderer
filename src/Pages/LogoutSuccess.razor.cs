using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages;

partial class LogoutSuccess : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private void GoToLoginPage()
    {
        NavigationManager.NavigateTo("/login");
    }
}
