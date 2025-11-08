using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class Sidebar : ComponentBase
{
    public enum JustifyContentOptions
    {
        Start,
        Center,
        End,
        Between,
        Around,
        Evenly
    }
    public enum OverflowOptions
    {
        Auto,
        Hidden,
        Scroll,
        Visible
    }

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public JustifyContentOptions JustifyContentType { get; set; } = JustifyContentOptions.Start;
    public string JustifyContent => JustifyContentType switch
    {
        JustifyContentOptions.Start => "justify-content-start",
        JustifyContentOptions.Center => "justify-content-center",
        JustifyContentOptions.End => "justify-content-end",
        JustifyContentOptions.Between => "justify-content-between",
        JustifyContentOptions.Around => "justify-content-around",
        JustifyContentOptions.Evenly => "justify-content-evenly",
        _ => "justify-content-start"
    };
    [Parameter] public OverflowOptions OverflowType { get; set; } = OverflowOptions.Scroll;
    public string Overflow => OverflowType switch
    {
        OverflowOptions.Auto => "overflow-auto",
        OverflowOptions.Hidden => "overflow-hidden",
        OverflowOptions.Scroll => "overflow-scroll",
        OverflowOptions.Visible => "overflow-visible",
        _ => "overflow-scroll"
    };
    [Parameter] public string Width { get; set; } = "250px";
}
