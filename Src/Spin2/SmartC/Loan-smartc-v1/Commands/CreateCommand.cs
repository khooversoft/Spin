using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loan_smartc_v1.Commands;

internal class CreateCommand : Command
{
    public CreateCommand() : base("create", "Create contract")
    {
    }
}
