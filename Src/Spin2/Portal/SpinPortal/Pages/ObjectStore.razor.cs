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
using Microsoft.Graph;
using Toolbox.Extensions;
using SpinPortal.Application;
using Toolbox.Types;

namespace SpinPortal.Pages;

public partial class ObjectStore
{
    [Parameter]
    public string? pageRoute { get; set; } = null!;

    [Inject]
    public PortalOption Option { get; set; } = null!;

    private ObjectUri _resolvedPath { get; set; } = null!;

    private string _textValue { get; set; } = null!;
    private bool _showResult { get; set; }

    protected override void OnParametersSet()
    {
        _showResult = false;

        _resolvedPath = ObjectUri.IsValid(pageRoute) switch
        {
            true => pageRoute.ToObjectUri() switch
            {
                var u when Option.Domains.Any(x => x == u.Domain) => u.ToString(),
                _ => Option.Domains.First(),
            },

            false => Option.Domains.First(),
        };
    }
}