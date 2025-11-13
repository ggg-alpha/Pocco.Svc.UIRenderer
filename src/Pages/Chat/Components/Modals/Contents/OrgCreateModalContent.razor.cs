using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Chat.Components.Modals.Contents;

public partial class OrgCreateModalContent : ComponentBase
{
    private const string OrgNameLengthErrorMessage = "組織名は30文字以内で入力してください。";
    private const string OrgDescOverMaxLengthErrorMessage = "組織説明は200文字以内で入力してください。";

    private string _orgName = string.Empty;
    private string _orgDesc = string.Empty;
    private bool _orgNameInvalid = false;
    private bool _orgDescInvalid = false;
    private bool _canSubmit => !_orgNameInvalid && !_orgDescInvalid;
    private string _orgNameErrorMessage = "string.Empty";
    private string _orgDescErrorMessage = "string.Empty";

    protected override async Task OnInitializedAsync()
    {
        _orgDescErrorMessage = string.Empty;
        _orgNameErrorMessage = string.Empty;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private void OnOrgNameChanged(ChangeEventArgs e)
    {
        _orgName = e.Value?.ToString() ?? string.Empty;
        _orgNameInvalid = string.IsNullOrWhiteSpace(_orgName);

        if (_orgNameInvalid)
        {
            _orgNameErrorMessage = "組織名は必須項目です。";
        }
        else if (_orgName.Length > 30)
        {
            _orgNameInvalid = true;
            _orgNameErrorMessage = OrgNameLengthErrorMessage;
        }
        else
        {
            _orgNameErrorMessage = string.Empty;
        }

        StateHasChanged();
    }

    private void OnOrgDescChanged(ChangeEventArgs e)
    {
        _orgDesc = e.Value?.ToString() ?? string.Empty;
        _orgDescInvalid = _orgDesc.Length > 200;

        if (_orgDescInvalid)
        {
            _orgDescErrorMessage = OrgDescOverMaxLengthErrorMessage;
        }
        else
        {
            _orgDescErrorMessage = string.Empty;
        }
        StateHasChanged();
    }

    private async Task SubmitCreateOrgAsync()
    {
        if (!_canSubmit)
        {
            Console.WriteLine("Cannot submit due to validation errors.");
            return;
        }

        Console.WriteLine($"Org Name: {_orgName} Desc: {_orgDesc}");

        await Task.CompletedTask;
    }
}
