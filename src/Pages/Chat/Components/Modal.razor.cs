using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Chat.Components;

public static class ModalMode
{
    public const string QuickAccess = "QuickAccess";
    public const string OrganizationSelector = "OrganizationSelector";
}

public partial class Modal : ComponentBase
{
    [Parameter] public bool IsShown { get; set; } = false;
    [Parameter] public Page ParentPage { get; set; } = null!;
    public string Mode { get; set; } = ModalMode.QuickAccess;

    public enum View { Page1, Page2, Page3 }

    View current = View.Page1;

    protected Type? currentType;
    protected Type? previousType;
    protected IDictionary<string, object>? currentParams;
    protected IDictionary<string, object>? previousParams;

    // CSSクラス名（アニメーション中のみ有効）
    string prevCss = "";
    string currCss = "";

    const int AnimationMs = 300;

    protected override void OnInitialized()
    {
        currentType = GetTypeFor(current);
        currentParams = GetParamsFor(current);
    }

    void Switch(View next)
    {
        if (next == current) return;

        // 保存して新しい fragment をセット
        previousType = currentType;
        previousParams = currentParams;

        // 進む／戻るの判定（enumの順序を利用）
        bool forward = next > current;

        // CSSクラスをセット（enter/exit）
        prevCss = forward ? "exit-left" : "exit-right";
        currCss = forward ? "enter-right" : "enter-left";

        current = next;
        currentType = GetTypeFor(current);
        currentParams = GetParamsFor(current);

        // アニメーション後に previous を消す
        _ = EndAnimation();
    }

    async Task EndAnimation()
    {
        await Task.Delay(AnimationMs);
        // アニメーション終わったら前のを消す
        previousType = null;
        previousParams = null;
        prevCss = currCss = "";
        StateHasChanged();
    }

    private Type GetTypeFor(View v) => v switch
    {
        View.Page1 => typeof(Chat.Components.Member),
        View.Page2 => typeof(Chat.Components.Icons),
        View.Page3 => typeof(Chat.Components.Header),
        _ => typeof(Chat.Components.Member)
    };

    private IDictionary<string, object>? GetParamsFor(View v)
    {
        // 例：Page2 に Title パラメータを渡す想定
        return v switch
        {
            View.Page2 => new Dictionary<string, object>
            {
                ["Mode"] = Chat.Components.Icons.IconMode.BootstrapIcons,
                ["IconSource"] = "chat-dots",
                ["Height"] = "32px",
                ["Width"] = "32px"
            },
            _ => null
        };
    }

    private async Task OnCloseClicked()
    {
        IsShown = false;

        await ParentPage.InvokeEventAsync(new InPageEventModel
        {
            EventType = InPageEventTypes.CloseModal
        });
    }
}
