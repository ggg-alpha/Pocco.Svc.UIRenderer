using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Client.Web.Clients;

public class AuthClient
{
    private readonly ILogger<AuthClient> _logger;
    private readonly V0ApiService.V0ApiServiceClient _v0Api;

    public AuthClient([FromServices] GrpcChannel channel, [FromServices] ILogger<AuthClient> logger)
    {
        _v0Api = new V0ApiService.V0ApiServiceClient(channel);
        _logger = logger;

        _logger.LogInformation("AuthClient initialized");
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

    // Authenticate
    public async Task<V0ApiSessionData> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var request = new V0AccountAuthenticateRequest
        {
            Email = email,
            Password = password
        };

        var response = await _v0Api.AuthenticateAsync(request, cancellationToken: cancellationToken);
        return response;
    }

    // Unauthenticate
    public async Task<Empty> UnauthenticateAsync(V0ApiSessionData session, CancellationToken cancellationToken = default)
    {
        var headers = CreateMetadata(session);
        var response = await _v0Api.UnauthenticateAsync(new Empty(), headers, cancellationToken: cancellationToken);
        return response;
    }
}
