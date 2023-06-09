using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using SpinPortal;
using SpinPortal.Shared;
using MudBlazor;
using Microsoft.Graph.ExternalConnectors;
using ObjectStore.sdk.Client;

namespace SpinPortal.Pages;

public partial class ObjectStore
{
    private string _textValue { get; set; } = null!;
    private bool _showResult { get; set; }

    protected override void OnParametersSet()
    {
        _showResult = false;
    }

    private void OnSearch()
    {
        _showResult = true;
        StateHasChanged();
    }

    private void HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            OnSearch();
        }
    }
}