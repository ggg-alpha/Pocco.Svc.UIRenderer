using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class OrganizationSelector : ComponentBase
{
    [Parameter] public Page ParentPage { get; set; } = null!;
    protected override async Task OnInitializedAsync()
    {
        ParentPage.OrgSelectorRef = this;

        await base.OnInitializedAsync();
    }

    private async Task OnOpenOrganizationSelectorClicked()
    {
        var eventModel = new InPageEventModel
        {
            EventType = InPageEventTypes.OpenModal
        };

        await ParentPage.InvokeEventAsync(eventModel);
    }
}
