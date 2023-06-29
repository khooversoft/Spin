using Microsoft.AspNetCore.Components;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Client;
using Toolbox.Types;

namespace SpinPortal.Pages;

public partial class Index
{
    [Inject] public SpinClusterClient SpinClusterClient { get; set; } = null!;
    [Inject] public ILogger<Index> Logger { get; set; } = null!;

    private string? _errorMessage;

    protected override async Task OnParametersSetAsync()
    {
        _errorMessage = null;

        Option<SiloConfigOption> option = await SpinClusterClient.Configuration.Get(new ScopeContext(Logger));
        if (option.IsError())
        {
            _errorMessage = $"Failed to get Spin configuration from server, statusCode={option.StatusCode}";
        }
    }
}
