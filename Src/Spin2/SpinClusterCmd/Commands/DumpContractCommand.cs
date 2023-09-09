using System.CommandLine;
using SpinClusterCmd.Activities;

namespace SpinClusterCmd.Commands;

internal class DumpContractCommand : Command
{
    public DumpContractCommand(DumpContract dumpContract) : base("dump", "Load scenario")
    {
        var accountId = new Argument<string>("contractId", "Contract ID to dump");
        var principalId = new Argument<string>("principalId", "Principal ID (ex. user@domain.com) that has rights to query contract");

        this.AddArgument(accountId);
        this.AddArgument(principalId);

        this.SetHandler(dumpContract.Dump, accountId, principalId);
    }
}
