using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Client;
using Toolbox.Types;

namespace SpinClusterCmd.Commands;

internal class UserCommand : CommandAbstract<UserModel>
{
    private readonly static DataType<UserModel> _dataType = new DataType<UserModel>
    {
        Name = "User",
        Validator = UserModelValidator.Validator,
        GetKey = x => x.UserId.ToObjectId(),
    };

    public UserCommand(SpinClusterClient client, ILogger<UserCommand> logger)
        : base("user", "Manage user information", _dataType, client, logger)
    {
    }
}
