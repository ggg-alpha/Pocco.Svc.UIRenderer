using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Pocco.Client.Web.Pages.Chat.Components;

public class ChatModel {
    public int Index { get; set; }
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsCategory { get; set; } = false;
    public string ParentId { get; set; } = string.Empty;

    private bool _isActivePlaceholder = false;
    public bool IsActive {
        get {
            if (IsCategory) {
                return false;
            }

            return _isActivePlaceholder;
        }
        set {
            _isActivePlaceholder = value;
        }
    }

    private bool _isExpandedPlaceholder = false;
    public bool IsExpanded {
        get {
            if (IsCategory) {
                return _isExpandedPlaceholder;
            }
            return false;
        }
        set {
            if (IsCategory) {
                _isExpandedPlaceholder = value;
            }
        }
    }
}

public partial class ChatList : ComponentBase {
    [Parameter] public Page ParentPage { get; set; } = null!;
    public List<ChatModel> Chats { get; set; } = new List<ChatModel> {
        new ChatModel {
            Index = 0,
            Id = "4e69fafa132a4e5ca5b2708c29e246d4",
            Name = "A",
            IsActive = true,
        },
        new ChatModel {
            Index = 2,
            Id = "70aa990908e7414bab60a2efb295c572",
            Name = "C",
            IsActive = false,
        },
        new ChatModel {
            Index = 1,
            Id = "8afb89270b6f4897b6171176667e99e8",
            Name = "B",
            IsActive = false,
        },
        new ChatModel { //! Category
            Index = 3,
            Id = "8afb89270b6f4897b6171176667e99e8",
            Name = "General",
            IsCategory = true,
            IsExpanded = true,
        },
        new ChatModel {
            Index = 4,
            Id = "e64cc39e8f834bc2809265fd196227c1",
            Name = "random",
            ParentId = "8afb89270b6f4897b6171176667e99e8",
            IsActive = false,
        },
        new ChatModel {
            Index = 0,
            Id = "01891625eded4b96960117e0c861eb0e",
            Name = "announcements",
            ParentId = "e64cc39e8f834bc2809265fd196227c1",
            IsActive = false,
        },
        new ChatModel { //! Category
            Index = 5,
            Id = "de1d54963a9f4e18b4f0c8b25eae22b5",
            Name = "Projects",
            IsCategory = true,
            IsExpanded = true,
        },
        new ChatModel {
            Index = 0,
            Id = "1601c72e943a45f9bbc7cc0103d55033",
            Name = "project-alpha",
            ParentId = "de1d54963a9f4e18b4f0c8b25eae22b5",
            IsActive = false,
        },
        new ChatModel {
            Index = 1,
            Id = "2c90ca12dc9947aab7c043226ade0d27",
            Name = "project-beta",
            ParentId = "de1d54963a9f4e18b4f0c8b25eae22b5",
            IsActive = false,
        },
    };
    private bool _expandCategory = true;

    protected override async Task OnInitializedAsync() {
        ParentPage.ChatListRef = this;

        // ソートをParentID→Indexの順に行う
        Chats = Chats
            .OrderBy(c => c.ParentId)
            .ThenBy(c => c.Index)
            .ToList();

        await base.OnInitializedAsync();
    }
    private async Task OnChatCategoryToggled(MouseEventArgs e) {
        _expandCategory = !_expandCategory;

        await InvokeAsync(StateHasChanged);
    }
    private async Task OnChatSelected(string id) {
        // 全てのチャットのIsActiveをfalseに設定
        foreach (var chat in Chats) {
            chat.IsActive = false;
        }

        // 選択されたチャットのIsActiveをtrueに設定
        var selectedChat = Chats.FirstOrDefault(c => c.Id == id);
        if (selectedChat != null) {
            selectedChat.IsActive = true;
        }

        await InvokeAsync(StateHasChanged);
    }
}
