using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Components;
using Pocco.Client.Web.Services;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.Client.Web.Pages.Register;

partial class Page : ComponentBase {
    private string email = string.Empty;
    private string emailError = string.Empty;

    private string password = string.Empty;
    private string passwordError = string.Empty;

    private bool hasEmailError = false;
    private bool hasPasswordError = false;


    [Inject] private APIClient.Core.APIClient ApiClient { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ProtectedLocalStorageProvider LocalStorageProvider { get; set; } = null!;
    [Inject] protected ILogger<Page> Logger { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            // Verify session
            var sessionData = await LocalStorageProvider.GetSessionDataAsync();
            if (sessionData is not null) {
                Logger.LogInformation($"Session data found for user ID: {sessionData.AccountId}");
                await ApiClient.SessionManager.VerifySessionAsync(sessionData);
                NavigationManager.NavigateTo("/chat/direct-messages");
            } else {
                Logger.LogWarning("No session data found in local storage. Staying on login page.");
            }

            Console.WriteLine("Chat Page Rendered");
        }
    }

    private async Task OnEmailInputChange(ChangeEventArgs e) {
        var value = e.Value?.ToString() ?? string.Empty;

        var isValid = string.IsNullOrWhiteSpace(value);
        isValid = !(value.Contains('@') && value.Contains('.'));

        emailError = isValid ? "Email is required." : string.Empty;
        hasEmailError = isValid;

        await Task.CompletedTask;
    }
    private async Task OnPasswordInputChange(ChangeEventArgs e) {
        var value = e.Value?.ToString() ?? string.Empty;

        var empty = string.IsNullOrWhiteSpace(value);
        passwordError = empty ? "Email is required." : string.Empty;
        hasPasswordError = empty;

        await Task.CompletedTask;
    }


    private async Task HandleRegister() {
        try {
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var passwordHash = SHA256.HashData(passwordBytes);
            var passwordHashString = Convert.ToBase64String(passwordHash);


            await ApiClient.CreateAccountAsync(new V0AccountRegisterRequest {
                Email = email,
                Password = passwordHashString
            });

            NavigationManager.NavigateTo("/login");
        } catch (Exception ex) {
            Logger.LogError($"Registration failed: {ex.Message}");
            return;
        }
    }

    private void GoToLoginPage() {
        NavigationManager.NavigateTo("/login");
    }
}
