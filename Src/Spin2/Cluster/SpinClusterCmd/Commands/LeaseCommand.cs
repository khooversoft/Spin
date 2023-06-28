using System.CommandLine;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Lease;
using SpinCluster.sdk.Client;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Commands;

internal class LeaseCommand : Command
{
    private readonly SpinClusterClient _client;
    private readonly ILogger<LeaseCommand> _logger;

    public LeaseCommand(SpinClusterClient client, ILogger<LeaseCommand> logger)
        : base("lease", "Acquire or release a lease")
    {
        _client = client.NotNull();
        _logger = logger.NotNull();

        AddCommand(AcquireCommand());
        AddCommand(ReleaseCommand());
    }

    private Command AcquireCommand()
    {
        var cmd = new Command("acquire", "Acquire a lease");

        Argument<string> idArgument = new Argument<string>("objectId", $"ObjectId of lease, syntax={ObjectId.Syntax}");

        cmd.AddArgument(idArgument);

        cmd.SetHandler(async (objectId) =>
        {
            var context = new ScopeContext(_logger);

            Toolbox.Types.Option<LeaseData> leaseData = await _client.Lease.Acquire(objectId.ToObjectId(), context);

            context.Location().Log(
                leaseData.IsOk() ? LogLevel.Information : LogLevel.Error,
                "Acquire objectId={objectId}, statusCode={statusCode}, leaseData={leaseData}",
                objectId, leaseData.StatusCode, leaseData.ToJsonPascalSafe(context));

        }, idArgument);

        return cmd;
    }

    private Command ReleaseCommand()
    {
        var cmd = new Command("release", "Release a lease");

        Argument<string> idArgument = new Argument<string>("objectId", $"ObjectId of lease, syntax={ObjectId.Syntax}");
        Argument<string> leaseIdArgument = new Argument<string>("leaseId", "LeaseId of lease to release");

        cmd.AddArgument(idArgument);
        cmd.AddArgument(leaseIdArgument);

        cmd.SetHandler(async (objectId, leaseId) =>
        {
            var context = new ScopeContext(_logger);

            StatusCode statusCode = await _client.Lease.Release(objectId.ToObjectId(), leaseId, context);

            context.Location().Log(statusCode.IsOk() ? LogLevel.Information : LogLevel.Error,
                "Release objectId={objectId}, leaseId={leaseId}, statusCode={statusCode}", objectId, leaseId, statusCode);

        }, idArgument, leaseIdArgument);

        return cmd;
    }
}
