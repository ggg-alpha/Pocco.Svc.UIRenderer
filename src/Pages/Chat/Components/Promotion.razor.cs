using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class Promotion
{
    [Parameter] public bool IsBordered { get; set; } = false;
}
