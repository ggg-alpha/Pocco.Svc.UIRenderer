using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Chat.Components;

public class ChatModel
{
    public string Name { get; set; } = "";
    public string Id { get; set; } = "";
    public Chats.ChatType Type { get; set; } = Chats.ChatType.Message;
    public int Index { get; set; } = 0;
}

public class CategoryModel : ChatModel
{
    public List<ChatModel>? Children { get; set; }
}

public partial class Chats : ComponentBase
{
    public enum ChatType
    {
        Message,
        Voice,
        Category,
        Future
    }

    [Parameter] public string Name { get; set; } = "";
    [Parameter] public string Id { get; set; } = "";
    [Parameter] public ChatType Type { get; set; } = ChatType.Message;
    [Parameter] public int Index { get; set; } = 0; // TODO: インデックスによる並べ替えは、将来的に実装予定（現在はAPIからのデータをそのまま反映する）
    [Parameter] public List<ChatModel> Children { get; set; } = new List<ChatModel>();

    public bool IsParent => Type == ChatType.Category;
    public List<ChatModel>? GetChildren() => IsParent ? Children : null;

    public string GetIcon() => Type switch
    {
        ChatType.Message => "chat-square-fill",
        ChatType.Voice => "mic-fill",
        ChatType.Category => "folder-fill",
        ChatType.Future => "question-lg",
        _ => "x-octagon"
    };
}
