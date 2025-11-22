using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Pocco.APIClient.Core;

namespace Pocco.Client.Web.Services;

public partial class GrpcClientFeeder : IDisposable
{
    public readonly Guid Id;
    public int ConnectionCount { get; private set; } = 0;
    public readonly APIClient.Core.APIClient ApiClient;

    private readonly ProtectedLocalStorageProvider _storage;
    private readonly ILogger<ComponentBase> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public GrpcClientFeeder(Guid id, ProtectedLocalStorageProvider localStorageProvider, ILogger<ComponentBase> logger)
    {
        _logger = logger;
        ApiClient = new APIClient.Core.APIClient(new APIClientConfigurations(APIClientType.User), logger);

        Id = id;
        _storage = localStorageProvider;
        _cancellationTokenSource = new CancellationTokenSource();

        // Try to get session data from storage
        try
        {
            var sessionData = _storage.GetSessionDataAsync().GetAwaiter().GetResult();
            if (sessionData is not null)
            {
                ApiClient.SessionManager.VerifySessionAsync(sessionData).GetAwaiter().GetResult();
                _logger.LogInformation("GrpcClientFeeder {Id} loaded session data from storage", Id);
            }
            else
            {
                _logger.LogInformation("GrpcClientFeeder {Id} found no session data in storage", Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GrpcClientFeeder {Id} failed to load session data from storage", Id);
        }

        ApiClient.EventHub.GetObservable<ClientEvents.OnClientLoggedIn>().Subscribe(async evt =>
        {
            _logger.LogInformation("GrpcClientFeeder {Id} received OnClientLoggedIn event", Id);
            await _storage.SetSessionDataAsync(evt.Session);
        }, _cancellationTokenSource.Token);
        ApiClient.EventHub.GetObservable<ClientEvents.OnSessionRefreshed>().Subscribe(async evt =>
        {
            _logger.LogInformation("GrpcClientFeeder {Id} received OnSessionRefreshed event", Id);
            await _storage.SetSessionDataAsync(evt.Session);
        }, _cancellationTokenSource.Token);

        _logger.LogInformation("GrpcClientFeeder {Id} created", Id);
    }

    public void IncrementConnectionCount() => ConnectionCount++;
    public void DecrementConnectionCount() => ConnectionCount--;

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        _logger.LogInformation("GrpcClientFeeder {Id} disposed", Id);

        GC.SuppressFinalize(this);
    }
}
