using Microsoft.AspNetCore.Mvc;

namespace Pocco.Client.Web.Services;

public class GrpcClientFeederProvider
{
    public readonly List<GrpcClientFeeder> GrpcClientFeeders = [];

    private readonly ILogger<GrpcClientFeederProvider> _logger;

    public GrpcClientFeederProvider([FromServices] ILogger<GrpcClientFeederProvider> logger)
    {
        _logger = logger;
        _logger.LogInformation("GrpcClientFeederProvider created");
    }

    public GrpcClientFeeder GetOrCreate(Guid id, Func<GrpcClientFeeder> createNew)
    {
        var existing = GrpcClientFeeders.FirstOrDefault(f => f.Id == id);
        if (existing != null)
        {
            return existing;
        }

        var newFeeder = createNew();
        GrpcClientFeeders.Add(newFeeder);
        return newFeeder;
    }

    public void Remove(Guid id)
    {
        var existing = GrpcClientFeeders.FirstOrDefault(f => f.Id == id);
        if (existing != null)
        {
            GrpcClientFeeders.Remove(existing);
            existing.Dispose();
        }
    }
}
