using Toolbox.Azure;

namespace Toolbox.Azure.test.Application;

internal record AzureTestOption
{
    public DatalakeOption Datalake { get; init; } = null!;
}

