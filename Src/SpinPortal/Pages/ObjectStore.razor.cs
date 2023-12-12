using Microsoft.AspNetCore.Components;
using SpinPortal.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinPortal.Pages;

public partial class ObjectStore
{
    [Inject] public PortalOption Option { get; set; } = null!;
    [Inject] public SpinClusterClient SpinClusterClient { get; set; } = null!;
    [Inject] public ILogger<ObjectStore> Logger { get; set; } = null!;

    [Parameter] public string? pageRoute { get; set; } = null!;

    private string? _resolvedPath { get; set; }
    private SiloConfigOption _siloConfigOption { get; set; } = null!;

    protected override async Task OnParametersSetAsync()
    {
        _siloConfigOption = (await SpinClusterClient.Configuration.Get(new ScopeContext(Logger)))
            .Assert(x => x.StatusCode.IsOk(), "Failed to get Spin configuration from Silo")
            .Return();

        _resolvedPath = ObjectId.IsValid(pageRoute) switch
        {
            true => ObjectId.Create(pageRoute).Return() switch
            {
                var u when exist(u.Schema, u.Tenant) => u.ToString(),
                _ => constructDefault(),
            },

            false => constructDefault(),
        };

        string constructDefault() => _siloConfigOption.Schemas.Select(x => x.SchemaName).First() + "/" + _siloConfigOption.Tenants[0];

        bool exist(string schema, string tenant) =>
            _siloConfigOption.Schemas.Any(x => x.SchemaName == schema) &&
            _siloConfigOption.Tenants.Any(x => x == tenant);
    }
}