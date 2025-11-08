using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class Member : ComponentBase
{
    public enum MemberStatus
    {
        Online,
        Offline,
        Busy,
        Away
    }

    [Parameter] public string IconUrl { get; set; } = "https://placehold.jp/150x150.png";
    [Parameter] public string Name { get; set; } = "Member Name";
    [Parameter] public string CustomStatusMessage { get; set; } = "This is a custom status message.";
    [Parameter] public MemberStatus Status { get; set; } = MemberStatus.Offline;

    private string GetStatusText()
    {
        return Status switch
        {
            MemberStatus.Online => "Online",
            MemberStatus.Offline => "Offline",
            MemberStatus.Busy => "Busy",
            MemberStatus.Away => "Away",
            _ => "Offline"
        };
    }

    private string GetStatusColor()
    {
        return Status switch
        {
            MemberStatus.Online => "green",
            MemberStatus.Offline => "gray",
            MemberStatus.Busy => "red",
            MemberStatus.Away => "yellow",
            _ => "gray"
        };
    }
}
