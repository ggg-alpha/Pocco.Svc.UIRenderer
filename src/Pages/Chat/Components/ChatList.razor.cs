using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Pocco.Client.Web.Pages.Chat.Components;

public class ChatModel {
    public int Index { get; set; }
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsCategory { get; set; } = false;
    public List<ChatModel> SubChats { get; set; } = [];
}

public partial class ChatList : ComponentBase {
    [Parameter] public Page ParentPage { get; set; } = null!;
    public List<ChatModel> Chats { get; set; } = new List<ChatModel> {
        new ChatModel {
            Index = 0,
            Id = "all-chats",
            Name = "A",
        },
        new ChatModel {
            Index = 2,
            Id = "direct-messages",
            Name = "C",
        },
        new ChatModel {
            Index = 1,
            Id = "direct-messages",
            Name = "B",
        },
        new ChatModel {
            Index = 3,
            Id = "category-1",
            Name = "General",
            IsCategory = true,
            SubChats = new List<ChatModel> {
                new ChatModel {
                    Index = 1,
                    Id = "chat-2",
                    Name = "random",
                },
                new ChatModel {
                    Index = 0,
                    Id = "chat-1",
                    Name = "announcements",
                },
            },
        },
        new ChatModel {
            Index = 4,
            Id = "category-2",
            Name = "Projects",
            IsCategory = true,
            SubChats = new List<ChatModel> {
                new ChatModel {
                    Index = 0,
                    Id = "chat-3",
                    Name = "project-alpha",
                },
                new ChatModel {
                    Index = 1,
                    Id = "chat-4",
                    Name = "project-beta",
                },
            },
        },
    };
    private bool _expandCategory = true;

    protected override async Task OnInitializedAsync() {
        ParentPage.ChatListRef = this;

        // IsCategoryでソート
        Chats = Chats.OrderByDescending(c => c.IsCategory).ToList();

        // Index通りにソート
        Chats = Chats.OrderBy(c => c.Index).ToList();
        foreach (var chat in Chats.Where(c => c.IsCategory)) {
            chat.SubChats = chat.SubChats.OrderBy(sc => sc.Index).ToList();
        }

        await base.OnInitializedAsync();
    }
    private async Task OnChatCategoryToggled(MouseEventArgs e) {
        _expandCategory = !_expandCategory;

        await InvokeAsync(StateHasChanged);
    }
}
