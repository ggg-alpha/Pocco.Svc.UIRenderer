using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class ChatHeader : ComponentBase
{
    [Parameter] public string ChatName { get; set; } = "Chat Header";
    [Parameter] public string Description { get; set; } = "";
    [Parameter] public Chats.ChatType Type { get; set; } = Chats.ChatType.Message;

    public string GetIcon() => Type switch
    {
        Chats.ChatType.Message => "chat-square-fill",
        Chats.ChatType.Voice => "mic-fill",
        Chats.ChatType.Category => "folder-fill",
        Chats.ChatType.Future => "question-lg",
        _ => "x-octagon"
    };
}
