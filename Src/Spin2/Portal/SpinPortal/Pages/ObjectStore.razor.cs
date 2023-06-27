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
using SpinCluster.sdk.Client;
using SpinCluster.sdk.Actors.Configuration;
using Toolbox.Tools;
using System.Threading.Tasks;
using Toolbox.Types.Id;

namespace SpinPortal.Pages;

public partial class ObjectStore
{
    [Inject] public PortalOption Option { get; set; } = null!;
    [Inject] public SpinConfigurationClient SpinConfigurationClient { get; set; } = null!;
    [Inject] public ILogger<ObjectStore> Logger { get; set; } = null!;

    [Parameter] public string? pageRoute { get; set; } = null!;

    private string _resolvedPath { get; set; } = "default";
    private SiloConfigOption _siloConfigOption { get; set; } = null!;

    private string _textValue { get; set; } = null!;
    private bool _showResult { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        _showResult = false;

        _siloConfigOption = (await SpinConfigurationClient.Get(new ScopeContext(Logger)))
            .Assert(x => x.IsOk(), "Failed to get Spin configuration from Silo")
            .Return();

        _resolvedPath = ObjectUri.IsValid(pageRoute) switch
        {
            true => pageRoute.ToObjectUri() switch
            {
                var u when _siloConfigOption.Schemas.Any(x => x.SchemaName == u.Domain) => u.ToString(),
                _ => _siloConfigOption.Schemas.Select(x => x.SchemaName).First(),
            },

            false => _siloConfigOption.Schemas.Select(x => x.SchemaName).First(),
        };
    }
}