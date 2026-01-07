using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Pocco.APIClient.Core;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class ChattingArea : ComponentBase {
    [Parameter] public Page ParentPage { get; set; } = null!;

    [Inject] public ILogger<ChattingArea> Logger { get; set; } = null!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private IConfiguration _configuration { get; set; } = null!;
    private string _cdnAddress { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync() {
        ParentPage.ChattingAreaRef = this;
        _cdnAddress = _configuration["CDN_ADDRESS"] ?? "http://localhost:5197";

        await base.OnInitializedAsync();
    }

    public async Task Initialize(bool firstRender) {
        if (firstRender) {

            try {
                await JSRuntime.InvokeVoidAsync("window.MessageContentHelper.clearMessages");

                var latestMessages = await ParentPage.ApiClient.ListOrganizationMessageAsync(new V0ListMessagesRequest {
                    OrganizationId = ParentPage.OrgId,
                    ChatId = ParentPage.ChatListRef!.CurrentChatId,
                    PageSize = 10,
                    PageNumber = 1
                });

                foreach (var message in latestMessages.Messages) {
                    var user = await ParentPage.ApiClient.GetAccountAsync(new V0AccountGetProfileRequest {
                        UserId = message.SenderId
                    });

                    var converted = message.Content.Replace("\\n", "\n");
                    await JSRuntime.InvokeVoidAsync(
                        "window.MessageContentHelper.createMessage",
                        $"{message.MessageId}",
                        $"{user.Username}",
                        converted,
                        $"{message.CreatedAt.ToDateTime().ToString("yyyy/MM/dd HH:mm:ss")}",
                        _cdnAddress
                    );
                }

                await InvokeAsync(StateHasChanged);

                await JSRuntime.InvokeVoidAsync("window.MessageContentHelper.scrollToBottom");
            } catch {
                Logger.LogError("Error handling while executing MessageContentHelper.markdownStringToHtml");
            }

            await InvokeAsync(StateHasChanged);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private string _input { get; set; } = string.Empty;
    private async Task OnMessageInputChanged(KeyboardEventArgs e) {
        Logger.LogInformation("Message input changed: {Key} | SHIFT: {Shift} | CTRL: {Ctrl} | ALT: {Alt} | META: {Meta}", e.Key, e.ShiftKey, e.CtrlKey, e.AltKey, e.MetaKey);

        if (e.Key == "Enter" && !e.ShiftKey) {
            var converted = _input.Replace("\r\n", "\\n")
                           .Replace("\r", "\\n")
                           .Replace("\n", "\\n");

            Logger.LogInformation("Sending message to chat...\nContent: {Content}", _input);

            var now = DateTime.UtcNow;
            try {
                var response = await ParentPage.ApiClient.CreateOrganizationMessageAsync(new Message {
                    OrganizationId = ParentPage.OrgId,
                    ChatId = ParentPage.ChatListRef!.CurrentChatId,
                    Content = converted,
                    CreatedAt = Timestamp.FromDateTime(now),
                    UpdatedAt = Timestamp.FromDateTime(now)
                });

                Logger.LogInformation("Message sent successfully, with event {EventId}", response.EventId);
                _input = string.Empty;
                await InvokeAsync(StateHasChanged);
            } catch {
                //
            }
        }

        await Task.CompletedTask;
    }

    // チャットを変更されたときに、メッセージを再読み込み
}
