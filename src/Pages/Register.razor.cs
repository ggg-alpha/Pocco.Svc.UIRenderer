using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages;

partial class Register : ComponentBase
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
