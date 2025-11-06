using System.Text.Json;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Client.Web.Services;

public partial class GrpcClientFeeder : IDisposable
{
    public readonly Guid Id;
    public int ConnectionCount { get; private set; } = 0;

    private readonly ILogger<ComponentBase> _logger;
    // private readonly Task _exampleLoopableTask;
    private readonly CancellationTokenSource _cancellationTokenSource;

    private readonly V0ApiService.V0ApiServiceClient _v0Api;

    public GrpcClientFeeder(Guid id, [FromServices] ILogger<ComponentBase> logger)
    {
        Id = id;
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
        // _exampleLoopableTask = Task.Run(() => ExampleLoopableTask(cts.Token), cts.Token);

        var apiConnectionString = Environment.GetEnvironmentVariable("API_CONNECTION_URL") ?? "https://localhost:7073";
        var channel = GrpcChannel.ForAddress(apiConnectionString);
        _v0Api = new V0ApiService.V0ApiServiceClient(channel);

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

    /// <summary>
    /// V0ApiSessionDataからgRPCのMetadataに変換する
    /// </summary>
    /// <param name="context">ページ内のContext</param>
    /// <returns><seealso cref="Metadata"/></returns>
    private static Metadata CreateMetadata(V0ApiSessionData data)
    {
        var createdAtStr = JsonSerializer.Serialize(data.CreatedAt);
        var expiresAtStr = JsonSerializer.Serialize(data.ExpiresAt);
        var updatedAtStr = JsonSerializer.Serialize(data.UpdatedAt);

        var metadata = new Metadata
        {
            { "Authorization", $"Bearer {data.Token}" },
            { "x-session-id", data.SessionId },
            { "x-account-id", data.AccountId },
            { "x-created-at", createdAtStr },
            { "x-expires-at", expiresAtStr },
            { "x-updated-at", updatedAtStr }
        };

        return metadata;
    }
}
