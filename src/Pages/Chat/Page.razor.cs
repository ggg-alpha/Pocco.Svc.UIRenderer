using Microsoft.AspNetCore.Components;
using Pocco.Client.Web.Pages.Chat.Components;

namespace Pocco.Client.Web.Pages.Chat;

public partial class Page : ComponentBase
{
    private readonly CategoryModel _sampleChatsList = new CategoryModel
    {
        Name = "カテゴリー1",
        Id = "category1",
        Type = Chats.ChatType.Category,
        Children = new List<ChatModel>
        {
            new ChatModel
            {
                Name = "チャットA",
                Id = "chatA",
                Type = Chats.ChatType.Message
            },
            new ChatModel
            {
                Name = "ボイスチャットB",
                Id = "voiceChatB",
                Type = Chats.ChatType.Voice
            }
        }
    };

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Console.WriteLine("Chat Page Rendered");
        }
    }
}
