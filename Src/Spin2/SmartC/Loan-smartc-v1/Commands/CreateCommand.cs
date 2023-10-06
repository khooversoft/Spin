using System.CommandLine;
using Loan_smartc_v1.Activitites;

namespace Loan_smartc_v1.Commands;

internal class CreateCommand : Command
{
    public CreateCommand(CreateContract createContract) : base("create", "Create contract")
    {
        var workId = new Option<string>("--workId", "WorkId to execute");

        AddOption(workId);
        this.SetHandler(createContract.Create, workId);
    }
}
