using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Client;
using SpinCluster.sdk.Directory.Models;

namespace SpinClusterCmd.Commands;

internal class UserCommand : CommandAbstract<UserPrincipal>
{
    private readonly static DataType<UserPrincipal> _dataType = new DataType<UserPrincipal>
    {
        Name = "User",
        Validator = UserPrincipalValidator.Validator,
        GetKey = x => x.UserId,
    };

    public UserCommand(SpinClusterClient client, ILogger<UserCommand> logger)
        : base("user", "Manage user information", _dataType, client, logger)
    {
    }
}
