using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pocco.Client.Web.Services;

namespace Pocco.Client.Web.Pages;

partial class Register : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private ILogger<Register> Logger { get; set; } = null!;
    [Inject] private GrpcClientFeederProvider ClientFeederProvider { get; set; } = null!;
    [Inject] private ProtectedLocalStorageProvider LocalStorageProvider { get; set; } = null!;
    private GrpcClientFeeder? _clientFeeder;

    private string email = string.Empty;
    private string emailError = string.Empty;

    private string password = string.Empty;
    private string passwordError = string.Empty;

    private bool isLoading = false;
    private bool hasEmailError = false;
    private bool hasPasswordError = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var scopedServiceId = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "scopedServiceId");
            var id = Guid.TryParse(scopedServiceId, out var guid) ? guid : Guid.NewGuid();
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "scopedServiceId", id.ToString());

            _clientFeeder = ClientFeederProvider.GetOrCreate(id, () => new GrpcClientFeeder(id, LocalStorageProvider, Logger));
        }
    }

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
        if (_clientFeeder == null)
        {
            Logger.LogError("GrpcClientFeeder is not initialized.");
            return;
        }

        try
        {
            await _clientFeeder.RegisterAccountAsync(email, password);

            NavigationManager.NavigateTo("/login");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Registration failed: {ex.Message}");
            return;
        }
    }

    private void GoToLoginPage()
    {
        NavigationManager.NavigateTo("/login");
    }
}
