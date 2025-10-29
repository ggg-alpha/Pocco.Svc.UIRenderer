using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Client.Web.Services;

public partial class GrpcClientFeeder : IDisposable
{
    public readonly Guid Id;

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

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _logger.LogInformation("GrpcClientFeeder {Id} disposed", Id);

        GC.SuppressFinalize(this);
    }

    private static Metadata ContextConvertToMetadata(HttpContext context)
    {
        var metadata = new Metadata();
        foreach (var header in context.Request.Headers)
        {
            if (header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrEmpty(header.Value))
            {
                continue;
            }

            metadata.Add(header.Key, header.Value.ToString());
        }
        return metadata;
    }
}
