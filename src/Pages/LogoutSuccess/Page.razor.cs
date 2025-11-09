using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.LogoutSuccess;

partial class Page : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private void GoToLoginPage()
    {
        NavigationManager.NavigateTo("/login");
    }
}
