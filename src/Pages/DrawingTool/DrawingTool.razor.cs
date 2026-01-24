using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Pocco.Client.Web.Pages.DrawingTool;

public partial class DrawingTool : ComponentBase, IAsyncDisposable {
    [Inject]
    public IJSRuntime JSRuntime { get; set; } = null!;

    private IJSObjectReference? _module;
    private IJSObjectReference? _drawingToolInstance;

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            // Import the JavaScript module. The path assumes Razor Class Library (RCL) static asset conventions.
            _module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/DrawingTool/DrawingTool.razor.js");

            // Initialize the drawing tool script and get the instance object back.
            _drawingToolInstance = await _module.InvokeAsync<IJSObjectReference>("init");
        }
    }

    // Implementation of IAsyncDisposable to clean up JS resources.
    public async ValueTask DisposeAsync() {
        if (_drawingToolInstance != null) {
            // Call the 'dispose' function on the JS side.
            await _drawingToolInstance.InvokeVoidAsync("dispose");
            await _drawingToolInstance.DisposeAsync();
        }

        if (_module != null) {
            await _module.DisposeAsync();
        }
    }
}
