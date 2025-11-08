using System.Text.Json;
using Grpc.Core;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Client.Web.Models;

public class SessionData
{
    public string SessionId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;
    public bool ShouldRefresh() => DateTime.UtcNow >= ExpiresAt.AddMinutes(-5);

    /// <summary>
    /// オブジェクトのクローンを作成する。これは、リクエスト送信中にデータが変更されるのを防ぐために使用する。
    /// </summary>
    /// <returns><seealso cref="SessionData"/></returns>
    public SessionData Clone()
    {
        return new SessionData
        {
            SessionId = this.SessionId,
            AccountId = this.AccountId,
            Token = this.Token,
            CreatedAt = this.CreatedAt,
            ExpiresAt = this.ExpiresAt,
            UpdatedAt = this.UpdatedAt
        };
    }

    /// <summary>
    /// オブジェクトの内容をJSON形式の文字列として返す
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        // Return a JSON for properties
        return JsonSerializer.Serialize(this);
    }

    public static SessionData FromV0ApiSessionData(V0ApiSessionData data)
    {
        return new SessionData
        {
            SessionId = data.SessionId,
            AccountId = data.AccountId,
            Token = data.Token,
            CreatedAt = data.CreatedAt.ToDateTime(),
            ExpiresAt = data.ExpiresAt.ToDateTime(),
            UpdatedAt = data.UpdatedAt.ToDateTime()
        };
    }

    public V0ApiSessionData ToV0ApiSessionData()
    {
        return new V0ApiSessionData
        {
            SessionId = this.SessionId,
            AccountId = this.AccountId,
            Token = this.Token,
            CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(this.CreatedAt.ToUniversalTime()),
            ExpiresAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(this.ExpiresAt.ToUniversalTime()),
            UpdatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(this.UpdatedAt.ToUniversalTime())
        };
    }

    public static Metadata ToMetadata(SessionData data)
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
