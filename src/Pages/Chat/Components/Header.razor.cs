using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class Header : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    // Parent -> This component
    // Parent HeaderCmp.EventNameCall -> This EventNameCall

    // This component -> Parent
    // This OnEventNameCall -> Parent.OnEventNameCall
}
