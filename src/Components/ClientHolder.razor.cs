using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.JSInterop;
using Pocco.Client.Web.Services;

namespace Pocco.Client.Web.Components;

public partial class ClientHolder : ComponentBase, IDisposable
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    public GrpcClientFeeder? Service;

    [Inject] protected ILogger<ClientHolder> Logger { get; set; } = null!;
    [Inject] protected CircuitHandler CircuitHandler { get; set; } = null!;

    [Inject] private GrpcClientFeederProvider ServiceProvider { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    private int connectedClientCount = 0;
    private int beforeClientCount = 0;
    private bool isDisposed = false;
    private Task _updateSupresserTask = null!;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Resolve connectedClients count
        var connectedClients = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "connectedClients");
        if (connectedClients == null)
        {
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "connectedClients", "0");
        }

        _ = int.TryParse(connectedClients, out connectedClientCount);

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

                    if (CircuitHandler is CircuitClosureDetector handler)
                    {
                        Logger.LogInformation($"Adding circuit disconnected handler for scoped service ID: {Service.Id}");

                        handler.Disconnected += async (circuit) => // TODO: ページを切り替えるごとにイベントが追加されてしまう問題を解決する
                        {
                            isDisposed = true;

                            Logger.LogInformation($"Circuit disconnected event triggered for scoped service ID: {Service.Id} (connected clients: {connectedClientCount})");

                            if (Service is not null && connectedClientCount == 0)
                            {
                                Logger.LogInformation($"Circuit disconnected. Removing scoped service ID: {Service.Id}");
                                ServiceProvider.Remove(Service.Id);
                                Service = null;
                            }
                        };
                    }

                    // increase client connected amount on localStorage
                    connectedClients = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "connectedClients");
                    if (int.TryParse(connectedClients, out var count))
                    {
                        count++;
                        await JSRuntime.InvokeVoidAsync("localStorage.setItem", "connectedClients", count.ToString());
                    }
                }
            }
            else
            {
                var newId = Guid.NewGuid();
                Service = ServiceProvider.GetOrCreate(newId, () => new GrpcClientFeeder(newId, Logger));

                // Store id in session storage
                await JSRuntime.InvokeVoidAsync("localStorage.setItem", "scopedServiceId", newId.ToString());

                // Add circuit association
                if (CircuitHandler is CircuitClosureDetector handler)
                {
                    handler.Disconnected += async (circuit) => // TODO: ページを切り替えるごとにイベントが追加されてしまう問題を解決する
                    {
                        isDisposed = true;

                        if (Service is not null && connectedClientCount == 0)
                        {
                            Logger.LogInformation($"Circuit disconnected. Removing scoped service ID: {Service.Id}");
                            ServiceProvider.Remove(Service.Id);
                            Service = null;
                        }
                    };
                }

                // increase client connected amount on localStorage
                connectedClients = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "connectedClients");
                if (int.TryParse(connectedClients, out var count))
                {
                    count++;
                    await JSRuntime.InvokeVoidAsync("localStorage.setItem", "connectedClients", count.ToString());
                }
            }

            StateHasChanged();
        }
    }

    private async Task UpdateSupresser()
    {
        do
        {
            // Resolve connectedClients count
            var connectedClients = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "connectedClients");
            if (connectedClients == null)
            {
                await JSRuntime.InvokeVoidAsync("localStorage.setItem", "connectedClients", "0");
            }

            _ = int.TryParse(connectedClients, out connectedClientCount);

            // Update state if connectedClientCount changed
            if (beforeClientCount != connectedClientCount)
            {
                Logger.LogInformation($"Connected clients changed from {beforeClientCount} to {connectedClientCount}");

                StateHasChanged();
            }

            beforeClientCount = connectedClientCount;

            await Task.Delay(1000, _cancellationTokenSource.Token);
        } while (!isDisposed);
    }

    public void Dispose()
    {
        isDisposed = true;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        GC.SuppressFinalize(this);
    }
}
