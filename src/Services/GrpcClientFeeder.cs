using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace Pocco.Client.Web.Services;

public class GrpcClientFeeder : IDisposable
{
    public readonly Guid Id;

    private readonly ILogger<ComponentBase> _logger;
    // private readonly Task _exampleLoopableTask;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public GrpcClientFeeder(Guid id, [FromServices] ILogger<ComponentBase> logger)
    {
        Id = id;
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
        // _exampleLoopableTask = Task.Run(() => ExampleLoopableTask(cts.Token), cts.Token);

        _logger.LogInformation("GrpcClientFeeder {Id} created", Id);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _logger.LogInformation("GrpcClientFeeder {Id} disposed", Id);

        GC.SuppressFinalize(this);
    }
}
