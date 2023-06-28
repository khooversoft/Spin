using Microsoft.AspNetCore.Components;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Client;
using SpinPortal.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinPortal.Pages;

public partial class ObjectStore
{
    [Inject] public PortalOption Option { get; set; } = null!;
    [Inject] public SpinConfigurationClient SpinConfigurationClient { get; set; } = null!;
    [Inject] public ILogger<ObjectStore> Logger { get; set; } = null!;

    [Parameter] public string? pageRoute { get; set; } = null!;

    private string _resolvedPath { get; set; } = "default";
    private SiloConfigOption _siloConfigOption { get; set; } = null!;

    protected override async Task OnParametersSetAsync()
    {
        _siloConfigOption = (await SpinConfigurationClient.Get(new ScopeContext(Logger)))
            .Assert(x => x.IsOk(), "Failed to get Spin configuration from Silo")
            .Return();

        _resolvedPath = ObjectId.IsValid(pageRoute) switch
        {
            true => pageRoute.ToObjectId() switch
            {
                var u when _siloConfigOption.Schemas.Any(x => x.SchemaName == u.Schema) => u.ToString(),
                _ => _siloConfigOption.Schemas.Select(x => x.SchemaName).First(),
            },

            false => _siloConfigOption.Schemas.Select(x => x.SchemaName).First(),
        };
    }
}