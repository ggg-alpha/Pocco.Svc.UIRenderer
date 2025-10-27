using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages;

partial class Login : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private string email = string.Empty;
    private string password = string.Empty;
    private bool isLoading = false;

    private void HandleLogin()
    {
        // Dummy login logic
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            // Handle validation error
            return;
        }

        // Set loading state
        isLoading = true;
        // Simulate login logic
        if (email is "s@g.com" && password is "123456")
        {
            // Navigate to home page on successful login
            NavigationManager.NavigateTo("/apps");
        }
        else
        {
            // Handle login failure (e.g., show error message)
        }
        isLoading = false;
    }

    private void GoToRegisterPage()
    {
        NavigationManager.NavigateTo("/register");
    }
}
