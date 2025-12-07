using Microsoft.AspNetCore.Components;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class OrgInfoCenter : ComponentBase {
    [Parameter] public Page ParentPage { get; set; } = null!;
    [Parameter] public string OrgId { get; set; } = string.Empty;

    private string _orgName = string.Empty;
    private string _orgDescription = string.Empty;

    protected override async Task OnInitializedAsync() {
        ParentPage.OrgInfoCenterRef = this;

        await base.OnInitializedAsync();
    }

    public async Task GetOrganizationInfo() {
        if (ParentPage.ApiClient is not null && !string.IsNullOrWhiteSpace(ParentPage.OrgId)) {
            try {
                var orgInfo = await ParentPage.ApiClient.GetOrganizationAsync(new V0BaseRequest {
                    Id = ParentPage.OrgId,
                });

                _orgName = orgInfo.Name;
                _orgDescription = orgInfo.Description;

                ParentPage.Logger.LogInformation("Fetched organization info for Org ID: {OrgId}, Name: {OrgName}", ParentPage.OrgId, _orgName);
            } catch (Exception ex) {
                ParentPage.Logger.LogError(ex, "Failed to fetch organization info for Org ID: {OrgId}", ParentPage.OrgId);
            }

            await InvokeAsync(StateHasChanged);
        }
    }
}
