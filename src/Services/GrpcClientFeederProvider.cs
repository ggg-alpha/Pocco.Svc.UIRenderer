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

    /// <summary>
    /// クライアントへの接続数をインクリメントする
    /// </summary>
    /// <remarks>
    /// 必ず、 OnInitializedAsync 内で呼び出すこと
    /// </remarks>
    /// <param name="deviceId"></param>
    public void InclementConnectionCount(Guid deviceId)
    {
        if (GrpcClientFeeders.FirstOrDefault(f => f.Id == deviceId) is GrpcClientFeeder client)
        {
            client.IncrementConnectionCount();

            _logger.LogInformation("GrpcClientFeeder {Id} incremented connection count -> {Count}", deviceId, client.ConnectionCount);
        }
    }

    /// <summary>
    /// クライアントへの接続数をデクリメントし、0以下になった場合はクライアントを削除する
    /// </summary>
    /// <remarks>
    /// 必ず、ページがDisposeされる際に呼び出すこと
    /// </remarks>
    /// <param name="deviceId"></param>
    public void DecrementConnectionCount(Guid deviceId)
    {
        if (GrpcClientFeeders.FirstOrDefault(f => f.Id == deviceId) is GrpcClientFeeder client)
        {
            client.DecrementConnectionCount();

            _logger.LogInformation("GrpcClientFeeder {Id} decremented connection count -> {Count}", deviceId, client.ConnectionCount);
        }

        var target = GrpcClientFeeders.FirstOrDefault(f => f.Id == deviceId);
        if (target is not null && target.ConnectionCount <= 0)
        {
            GrpcClientFeeders.Remove(target);
            target.Dispose();

            _logger.LogInformation("GrpcClientFeeder {Id} removed due to zero connections", deviceId);
        }
    }
}
