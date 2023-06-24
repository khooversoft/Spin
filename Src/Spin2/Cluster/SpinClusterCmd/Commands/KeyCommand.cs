using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Directory;
using SpinCluster.sdk.Client;

namespace SpinClusterCmd.Commands;

internal class KeyCommand : CommandAbstract<PrincipalKey>
{
    private readonly static DataType<PrincipalKey> _dataType = new DataType<PrincipalKey>
    {
        Name = "Key",
        Validator = PrincipalKeyValidator.Validator,
        GetKey = x => x.UserId,
    };

    public KeyCommand(SpinClusterClient client, ILogger<KeyCommand> logger)
        : base("key", "Manage key information", _dataType, client, logger)
    {
    }
}
