using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Chat;

public partial class Page : ComponentBase
{
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Console.WriteLine("Chat Page Rendered");
        }
    }
}
