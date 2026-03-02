using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Graph.Grant;

public class GrantControlTests
{
    [Fact]
    public void Empty()
    {
        ReaderWriterLockSlim slimLock = new ReaderWriterLockSlim();

        var control = new GrantControl(slimLock, NullLogger.Instance);
        control.PrincipalRegistry.GetAll().Count.Be(0);
        control.GrantPolicyRegistry.GetAll().Count.Be(0);
    }
}
