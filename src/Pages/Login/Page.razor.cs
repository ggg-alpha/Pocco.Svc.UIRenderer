using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pocco.Client.Web.Services;

namespace Pocco.Client.Web.Pages.Login;

partial class Page : ComponentBase {
    private string email = string.Empty;
    private string emailError = string.Empty;

    private string password = string.Empty;
    private string passwordError = string.Empty;

    private bool isLoading = false;
    private bool hasEmailError = false;
    private bool hasPasswordError = false;

    [Inject] private APIClient.Core.APIClient ApiClient { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ProtectedLocalStorageProvider LocalStorageProvider { get; set; } = null!;
    [Inject] protected ILogger<Page> Logger { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            ApiClient.EventHub.GetObservable<Pocco.APIClient.Core.ClientEvents.OnClientLoggedIn>().Subscribe(async (evt) => // ! ブラウザ共有の影響で、lspが別サーキットになると動作しない
            {
                Logger.LogInformation("GrpcClientFeeder received OnClientLoggedIn event");
                await LocalStorageProvider.SetSessionDataAsync(evt.Session);
            });

            // Verify session
            var sessionData = await LocalStorageProvider.GetSessionDataAsync();
            if (sessionData is not null) {
                Logger.LogInformation($"Session data found for user ID: {sessionData.AccountId}");
                try {
                    var result = await ApiClient.SessionManager.VerifySessionAsync(sessionData);
                    if (result is true) {
                        NavigationManager.NavigateTo("/chat");
                    }
                } catch (Exception ex) {
                    Logger.LogError(ex, "Failed to verify session data.");
                }
            } else {
                Logger.LogWarning("No session data found in local storage. Staying on login page.");
            }
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

    private async Task HandleLogin() {
        // Dummy login logic
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) {
            // Handle validation error
            return;
        }

        // Set loading state
        isLoading = true;

        try {
            var passwordHash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password)).ToString();

            if (passwordHash is null) {
                return;
            }

            var isSuccess = await ApiClient.SessionManager.LoginAsync(email, passwordHash);

            if (isSuccess) {
                Logger.LogInformation("Login successful.");

                var currentSession = ApiClient.SessionManager.GetSessionData();

                if (currentSession is null) {
                    hasEmailError = true;
                    hasPasswordError = true;

                    Logger.LogError("Login failed: Session data is null after successful login.");
                    return;
                }

                Logger.LogInformation($"Current session data: {JsonSerializer.Serialize(currentSession)}");
                try {
                    Logger.LogInformation("Storing session data to local storage.");
                    await LocalStorageProvider.SetSessionDataAsync(currentSession);
                } catch (Exception ex) {
                    Logger.LogError(ex, "Failed to store session data to local storage.");
                    throw;
                }
            } else {
                throw new Exception("Login failed due to invalid credentials.");
            }

            NavigationManager.NavigateTo("/chat");
        } catch (Exception ex) {
            emailError = "Invalid email.";
            hasEmailError = true;
            passwordError = "Invalid password.";
            hasPasswordError = true;

            Console.WriteLine($"Login failed: {ex.Message}");
        } finally {
            isLoading = false;
        }
    }

    private void GoToRegisterPage() {
        NavigationManager.NavigateTo("/register");
    }
}
