using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Pocco.Libs.Protobufs.CoreAPI.Services;

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
    public List<ChatModel> Chats { get; set; } = [];
    public string CurrentChatId { get; set; } = string.Empty;

    private bool _expandCategory = true;
    [Inject] private IConfiguration _configuration { get; set; } = null!;
    private string _cdnAddress { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync() {
        ParentPage.ChatListRef = this;
        _cdnAddress = _configuration["CDN_ADDRESS"] ?? "http://localhost:5197";

        // ソートをParentID→Indexの順に行う
        Chats = Chats
            .OrderBy(c => c.ParentId)
            .ThenBy(c => c.Index)
            .ToList();

        await base.OnInitializedAsync();
    }

    public async Task InitializeAsync() {
        var dbChatList = await ParentPage.ApiClient.ListOrganizationChatsAsync(new V0ListXRequest {
            Base = new V0BaseRequest {
                Id = ParentPage.OrgId,
            },
        });

        var chatList = new List<ChatModel>();
        var index = 0;
        foreach (var item in dbChatList.Chats) {
            chatList.Add(new ChatModel {
                Index = index,
                Id = item.ChatId,
                Name = item.Name,
                IsActive = false,
            });
            index++;
        }

        if (chatList.Count == 0) {
            return;
        }

        Chats = chatList;
        CurrentChatId = Chats.FirstOrDefault()?.Id ?? string.Empty;

        Chats.First().IsActive = true;

        ParentPage.Logger.LogInformation("Fetched chat list for Org ID: {OrgId}", ParentPage.OrgId);
        ParentPage.Logger.LogInformation("Current chat ID: {ChatId}", CurrentChatId);

        await InvokeAsync(StateHasChanged);
    }

    private async Task OnChatCategoryToggled(MouseEventArgs e) {
        _expandCategory = !_expandCategory;

        await InvokeAsync(StateHasChanged);
    }
    private async Task OnChatSelected(string id) {
        if (CurrentChatId == id) {
            return;
        }

        CurrentChatId = id;

        // 全てのチャットのIsActiveをfalseに設定
        foreach (var chat in Chats) {
            chat.IsActive = false;
        }

        // 選択されたチャットのIsActiveをtrueに設定
        var selectedChat = Chats.FirstOrDefault(c => c.Id == id);
        if (selectedChat != null) {
            selectedChat.IsActive = true;
            CurrentChatId = id;

            ParentPage.Logger.LogInformation("Selected chat: {ChatId}", id);
        }

        // チャットエリアを再初期化
        await ParentPage.ChattingAreaRef!.Initialize(true);

        await InvokeAsync(StateHasChanged);
    }
}
