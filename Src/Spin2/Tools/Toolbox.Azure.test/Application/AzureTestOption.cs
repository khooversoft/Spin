using Microsoft.Extensions.Configuration.UserSecrets;
using Toolbox.Azure.DataLake;

namespace Toolbox.Azure.test.Application;

internal record AzureTestOption
{
    public DatalakeOption Datalake { get; init; } = null!;
}

