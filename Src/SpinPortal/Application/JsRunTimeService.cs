using Microsoft.JSInterop;

namespace SpinPortal.Application;

public class JsRunTimeService
{
    private readonly IJSRuntime _jsRuntime;

    public JsRunTimeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ValueTask WriteToClipboard(string text)
    {
        return _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }

    public async Task DownloadFile(string fileName, byte[] content)
    {
        using var fileStream = new MemoryStream(content);
        using var streamRef = new DotNetStreamReference(stream: fileStream);

        await _jsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }
}
