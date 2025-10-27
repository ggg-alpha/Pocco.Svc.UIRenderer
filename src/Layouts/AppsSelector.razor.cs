using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Layouts;

partial class AppsSelector
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private void NavigateToChat()
    {
        NavigationManager.NavigateTo("/apps/chat");
    }

    private void NavigateToCalendar()
    {
        NavigationManager.NavigateTo("/apps/calendar");
    }

    private void NavigateToSpreadsheet()
    {
        NavigationManager.NavigateTo("/apps/spreadsheet");
    }

    private void NavigateToWhiteboard()
    {
        NavigationManager.NavigateTo("/apps/whiteboard");
    }

    private void NavigateToVideo()
    {
        NavigationManager.NavigateTo("/apps/video");
    }

    private void NavigateToPhoto()
    {
        NavigationManager.NavigateTo("/apps/photo");
    }

    private void NavigateToForum()
    {
        NavigationManager.NavigateTo("/apps/forum");
    }
}
