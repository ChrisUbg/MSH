using System.Text.Json;
using Microsoft.JSInterop;

namespace MSH.Web.Services;

public class ChartInteropService
{
    private readonly IJSRuntime _jsRuntime;

    public ChartInteropService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<object> CreateChartAsync(string canvasId, string type, object data, object options)
    {
        return await _jsRuntime.InvokeAsync<object>("chartInterop.createChart", canvasId, type, data, options);
    }

    public async Task UpdateChartAsync(string chartId, object data)
    {
        await _jsRuntime.InvokeVoidAsync("chartInterop.updateChart", chartId, data);
    }

    public async Task DestroyChartAsync(string chartId)
    {
        await _jsRuntime.InvokeVoidAsync("chartInterop.destroyChart", chartId);
    }
} 