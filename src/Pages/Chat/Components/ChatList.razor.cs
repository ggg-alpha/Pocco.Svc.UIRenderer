using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class ChatList : ComponentBase {
    [Parameter] public Page ParentPage { get; set; } = null!;
    private bool _expandCategory = true;

    protected override async Task OnInitializedAsync() {
        await base.OnInitializedAsync();
    }
    private async Task OnChatCategoryToggled(MouseEventArgs e) {
        _expandCategory = !_expandCategory;

        await InvokeAsync(StateHasChanged);
    }
}
