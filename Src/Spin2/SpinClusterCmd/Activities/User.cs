using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.User;
using Toolbox.Tools;

namespace SpinClusterCmd.Activities;

internal class User
{
    private readonly UserClient _client;
    private readonly ILogger<User> _logger;

    public User(UserClient client, ILogger<User> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }
}
