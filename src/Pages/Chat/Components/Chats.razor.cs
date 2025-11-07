using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class Chats : ComponentBase
{
    public enum ChatType
    {
        Message,
        Voice,
        Category,
        Future
    }

    [Parameter] public string? Name { get; set; } = "";
    [Parameter] public ChatType Type { get; set; } = ChatType.Message;
    [Parameter] public int Index { get; set; } = 0; // TODO: インデックスによる並べ替えは、将来的に実装予定（現在はAPIからのデータをそのまま反映する）
    [Parameter] public List<object>? Children { get; set; }

    public bool IsParent => Type == ChatType.Category;
    public List<object> GetChildren() => IsParent && Children != null ? Children : new List<object>();

    public string GetIcon() => Type switch
    {
        ChatType.Message => "chat-square-fill",
        ChatType.Voice => "mic-fill",
        ChatType.Category => "folder-fill",
        ChatType.Future => "question-lg",
        _ => "x-octagon"
    };
}
