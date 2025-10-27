using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages;

partial class Register : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private string email = string.Empty;
    private string password = string.Empty;
    // private bool isLoading = false;

    private async Task HandleRegister()
    {
        // isLoading = true;
        // Simulate registration logic
        await Task.Delay(1000);
        // isLoading = false;
        // Redirect or show success message
    }

    private void GoToLoginPage()
    {
        NavigationManager.NavigateTo("/login");
    }
}
