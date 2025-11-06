using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pocco.Client.Web.Services;

namespace Pocco.Client.Web.Components;

public partial class ClientHolder : ComponentBase, IDisposable
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    public GrpcClientFeeder? Service;

    [Inject] protected ILogger<ClientHolder> Logger { get; set; } = null!;

    [Inject] private GrpcClientFeederProvider ServiceProvider { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    private int beforeClientCount = 0;
    private Task _updateSupresserTask = null!;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _updateSupresserTask = UpdateSupresser();

            // Get id from session storage (if exists)
            var id = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "scopedServiceId");
            if (!string.IsNullOrEmpty(id) && Guid.TryParse(id, out var guid))
            {
                Service = ServiceProvider.GetOrCreate(guid, () => new GrpcClientFeeder(guid, Logger));
                if (Service is not null)
                {
                    Logger.LogInformation($"Retrieved existing scoped service ID from session storage: {Service.Id}");

                    Service.IncrementConnectionCount();
                }
            }
            else
            {
                var newId = Guid.NewGuid();
                Service = ServiceProvider.GetOrCreate(newId, () => new GrpcClientFeeder(newId, Logger));
                Service.IncrementConnectionCount();
            }

            StateHasChanged();
        }
    }

    private async Task UpdateSupresser()
    {
        do
        {
            // Update state if connectedClientCount changed
            if (beforeClientCount != Service?.ConnectionCount)
            {
                Logger.LogInformation($"Connected clients changed from {beforeClientCount} to {Service?.ConnectionCount}");

                StateHasChanged();
            }

            beforeClientCount = Service?.ConnectionCount ?? 0;

            await Task.Delay(1000, _cancellationTokenSource.Token);
        } while (!_cancellationTokenSource.IsCancellationRequested);
    }

    public void Dispose()
    {
        Service?.DecrementConnectionCount();

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        Logger.LogInformation("ClientHolder disposed");

        GC.SuppressFinalize(this);
    }
}
