﻿using Microsoft.JSInterop;

namespace SpinPortal.Application;

public class ClipboardService
{
    private readonly IJSRuntime _jsRuntime;

    public ClipboardService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ValueTask WriteTextAsync(string text)
    {
        return _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }
}
