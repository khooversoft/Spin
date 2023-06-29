using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Directory;
using SpinCluster.sdk.Client;
using Toolbox.Types;

namespace SpinClusterCmd.Commands;

internal class UserCommand : CommandAbstract<UserPrincipal>
{
    private readonly static DataType<UserPrincipal> _dataType = new DataType<UserPrincipal>
    {
        Name = "User",
        Validator = UserPrincipalValidator.Validator,
        GetKey = x => x.UserId.ToObjectId(),
    };

    public UserCommand(SpinClusterClient client, ILogger<UserCommand> logger)
        : base("user", "Manage user information", _dataType, client, logger)
    {
    }
}
