using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Client.Web.Services;

public partial class GrpcClientFeeder
{
    public V0ApiService.V0ApiServiceClient V0Api => _v0Api;

    /// <summary>
    /// アカウントを登録するためのラッパーメソッド
    /// </summary>
    /// <param name="email">登録したいEmailアドレス</param>
    /// <param name="password">登録したいパスワード</param>
    /// <param name="cancellationToken">タスクのキャンセル</param>
    /// <returns><seealso cref="Empty"/></returns>
    public async Task<Empty> RegisterAccountAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var request = new V0AccountRegisterRequest
        {
            Email = email,
            Password = password
        };

        var response = await _v0Api.RegisterAsync(request, cancellationToken: cancellationToken);
        return response;
    }

    /// <summary>
    /// アカウントを削除するためのラッパーメソッド
    /// </summary>
    /// <param name="cancellationToken">タスクのキャンセル</param>
    /// <returns><seealso cref="Empty"/></returns>
    public async Task<Empty> UnregisterAccountAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        var headers = ContextConvertToMetadata(context);
        var response = await _v0Api.DeleteAccountAsync(new Empty(), headers, cancellationToken: cancellationToken);
        return response;
    }
}
