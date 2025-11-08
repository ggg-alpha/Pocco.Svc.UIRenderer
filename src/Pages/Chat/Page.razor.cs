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
    private readonly List<ChatItem> _sampleMessages = new List<ChatItem>
    {
        new ChatItem
        {
            Id = "msg1",
            AuthorId = "user1",
            Content = "こんにちは！",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        },
        new ChatItem
        {
            Id = "msg2",
            AuthorId = "user2",
            Content = "こんばんは！",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        },
        new ChatItem
        {
            Id = "msg3",
            AuthorId = "user1",
            Content = "元気ですか？",
            CreatedAt = DateTime.UtcNow.AddMinutes(-2)
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
