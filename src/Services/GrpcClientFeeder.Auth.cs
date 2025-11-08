using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Pocco.Client.Web.Models;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Client.Web.Services;

public partial class GrpcClientFeeder
{
    public async Task<SessionData> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var request = new V0AccountAuthenticateRequest
        {
            Email = email,
            Password = password
        };

        var response = await _v0Api.AuthenticateAsync(request, cancellationToken: cancellationToken);
        return SessionData.FromV0ApiSessionData(response);
    }

    public async Task<SessionData> RefreshSessionAsync(SessionData currentSession, CancellationToken cancellationToken = default)
    {
        var headers = SessionData.ToMetadata(currentSession);
        var response = await _v0Api.VerifyTokenAsync(new Empty(), headers, cancellationToken: cancellationToken);
        return SessionData.FromV0ApiSessionData(response);
    }

    public async Task SignOutAsync(SessionData currentSession, CancellationToken cancellationToken = default)
    {
        var headers = SessionData.ToMetadata(currentSession);
        var response = await _v0Api.UnauthenticateAsync(new Empty(), headers, cancellationToken: cancellationToken);
        await Task.CompletedTask;
    }

    private async Task RefreshSessionTask(CancellationToken cancellationToken = default)
    {
        _currentSessionData = await _storage.GetSessionDataAsync();

        while (!cancellationToken.IsCancellationRequested)
        {
            if (_currentSessionData is null || _currentSessionData.ShouldRefresh() is false)
            {
                continue;
            }

            try
            {
                var refreshedSession = await RefreshSessionAsync(_currentSessionData, cancellationToken);
                await _storage.SetSessionDataAsync(refreshedSession);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to refresh session data in GrpcClientFeeder {Id}", Id);
            }

            await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
        }
    }
}
