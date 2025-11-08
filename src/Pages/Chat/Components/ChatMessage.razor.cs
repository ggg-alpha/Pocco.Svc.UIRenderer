using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class ChatMessage : ComponentBase
{
    [Parameter] public ChatItem Message { get; set; } = new ChatItem();
}
