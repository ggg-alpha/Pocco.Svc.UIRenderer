using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Chat.Components;

public partial class Icons : ComponentBase
{
    public enum IconMode

    {
        BootstrapIcons,
        Images
    }

    [Parameter] public IconMode Mode { get; set; } = IconMode.BootstrapIcons;
    [Parameter] public string IconSource { get; set; } = string.Empty;
    /// <summary>
    /// カーソルスタイル
    /// </summary>
    [Parameter] public string Cursor { get; set; } = "pointer";

    // -- Bootstrap Icons 専用パラメータ --
    /// <summary>
    /// アイコンの色 (Bootstrap Icons 専用)
    /// </summary>
    [Parameter] public string? Color { get; set; }

    // TODO: クリック時のイベントハンドラの追加
    // [Parameter] public EventCallback OnClick { get; set; }
    // @onclick="OnClick.InvokeAsync" -> 親要素 -> click,{event},{tag} のようにしてページ単位でハンドルできるようにする
}
