using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Graph.Grant;

public class GrantControlTests
{
    [Fact]
    public void Empty()
    {
        var control = new GrantControl(NullLogger.Instance);
        control.PrincipalRegistry.GetAll().Count.Be(0);
        control.GrantPolicyRegistry.GetAll().Count.Be(0);
    }
}
