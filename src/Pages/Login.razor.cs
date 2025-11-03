using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pocco.Client.Web.Clients;

namespace Pocco.Client.Web.Pages;

partial class Login : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ILogger<Login> Logger { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private AuthClient AuthClient { get; set; } = null!;

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

    private async Task HandleLogin()
    {
        // Dummy login logic
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            // Handle validation error
            return;
        }

        // Set loading state
        isLoading = true;

        try
        {
            var response = await AuthClient.AuthenticateAsync(email, password);

            // Store session data to local storage
            var responseStr = JsonSerializer.Serialize(response);
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "sessionData", responseStr);

            NavigationManager.NavigateTo("/apps");
        }
        catch (Exception ex)
        {
            emailError = "Invalid email.";
            hasEmailError = true;
            passwordError = "Invalid password.";
            hasPasswordError = true;

            Console.WriteLine($"Login failed: {ex.Message}");
        }
        finally
        {
            isLoading = false;
        }
    }

    private void GoToRegisterPage()
    {
        NavigationManager.NavigateTo("/register");
    }
}
