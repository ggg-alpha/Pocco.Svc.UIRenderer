using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages;

partial class Login : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private string email = string.Empty;
    private string emailError = string.Empty;

    private string password = string.Empty;
    private string passwordError = string.Empty;

    private bool isLoading = false;
    private bool hasEmailError = false;
    private bool hasPasswordError = false;

    private async Task OnEmailInputChange(ChangeEventArgs e)
    {
        var value = e.Value?.ToString() ?? string.Empty;

        var isValid = string.IsNullOrWhiteSpace(value);
        isValid = !(value.Contains('@') && value.Contains('.'));

        emailError = isValid ? "Email is required." : string.Empty;
        hasEmailError = isValid;

        await Task.CompletedTask;
    }
    private async Task OnPasswordInputChange(ChangeEventArgs e)
    {
        var value = e.Value?.ToString() ?? string.Empty;

        var empty = string.IsNullOrWhiteSpace(value);
        passwordError = empty ? "Email is required." : string.Empty;
        hasPasswordError = empty;

        await Task.CompletedTask;
    }

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
