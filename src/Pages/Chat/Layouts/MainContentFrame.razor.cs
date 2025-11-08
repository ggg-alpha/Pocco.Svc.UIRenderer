using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Chat.Layouts;

public partial class MainContentFrame : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
