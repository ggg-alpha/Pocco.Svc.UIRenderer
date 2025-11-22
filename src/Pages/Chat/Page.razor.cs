using Microsoft.AspNetCore.Components;
using Pocco.Client.Web.Pages.Chat.Components;
using Pocco.Client.Web.Services;

namespace Pocco.Client.Web.Pages.Chat;

public class InPageEventModel
{
    public string EventType { get; set; } = string.Empty;
}

public static class InPageEventTypes
{
    public const string OpenModal = "OpenModal";
    public const string CloseModal = "CloseModal";
}

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
    private bool _isModalShown = false;
    private GrpcClientFeeder Service { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private GrpcClientFeederProvider ServiceProvider { get; set; } = null!;
    [Inject] private ProtectedLocalStorageProvider LocalStorageProvider { get; set; } = null!;
    [Inject] protected ILogger<Page> Logger { get; set; } = null!;

    public OrganizationSelector? OrgSelectorRef;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Get id from session storage (if exists)
            var id = await LocalStorageProvider.GetDeviceIdAsync().ConfigureAwait(false);
            if (id is Guid guid)
            {
                Service = ServiceProvider.GetOrCreate(guid, () => new GrpcClientFeeder(guid, LocalStorageProvider, Logger));
                if (Service is not null)
                {
                    Logger.LogInformation($"Retrieved existing scoped service ID from session storage: {Service.Id}");

                    Service.IncrementConnectionCount();
                }
            }
            else
            {
                Logger.LogInformation("No existing scoped service ID found in session storage. Creating new one.");

                var newId = Guid.NewGuid();
                Service = ServiceProvider.GetOrCreate(newId, () => new GrpcClientFeeder(newId, LocalStorageProvider, Logger));
                Service.IncrementConnectionCount();

                await LocalStorageProvider.SetDeviceIdAsync(newId);

                Logger.LogInformation($"Created new scoped service ID: {Service.Id}");
            }

            if (Service is null)
            {
                Logger.LogError("GrpcClientFeeder service is null. Cannot verify session.");
                // NavigationManager.NavigateTo("/error");
                return;
            }

            // Verify session
            var sessionData = await LocalStorageProvider.GetSessionDataAsync();
            if (sessionData is null)
            {
                Logger.LogWarning("No session data found in local storage. Redirecting to login page.");
                NavigationManager.NavigateTo("/login");
            }
            else
            {
                Logger.LogInformation($"Session data found for user ID: {sessionData.AccountId}");

                await Service.ApiClient.SessionManager.VerifySessionAsync(sessionData);
            }

            Console.WriteLine("Chat Page Rendered");
        }
    }

    public async Task InvokeEventAsync(InPageEventModel e)
    {
        if (e.EventType == InPageEventTypes.OpenModal)
        {
            // モーダルを開く処理をここに実装
            _isModalShown = true;
            StateHasChanged();
            Console.WriteLine("Open Modal Event Invoked");
        }
        else if (e.EventType == InPageEventTypes.CloseModal)
        {
            // モーダルを閉じる処理をここに実装
            _isModalShown = false;
            StateHasChanged();
            Console.WriteLine("Close Modal Event Invoked");
        }

        await Task.CompletedTask;
    }
}
