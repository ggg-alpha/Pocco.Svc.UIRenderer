using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class ChattingArea : ComponentBase {
    [Inject] public ILogger<ChattingArea> Logger { get; set; } = null!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = null!;

    private string content = "# Test\\nNew line";

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            try {
                var converted = content.Replace("\\n", "\n");
                await JSRuntime.InvokeVoidAsync(
                    "window.MessageContentHelper.markdownStringToHtml",
                    converted,
                    "1"
                );
            } catch {
                Logger.LogError("Error handling while executing MessageContentHelper.markdownStringToHtml");
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}
