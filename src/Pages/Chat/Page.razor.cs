using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Chat;

public partial class Page : ComponentBase
{
    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine("Chat Page Initialized");

        await base.OnInitializedAsync();
    }
}
