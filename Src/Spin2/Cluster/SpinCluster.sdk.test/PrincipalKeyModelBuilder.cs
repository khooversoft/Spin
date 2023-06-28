using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.Directory;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinCluster.sdk.test;

public class PrincipalKeyModelBuilder
{
    [Fact]
    public void CreateKeyData()
    {
        var model = PrincipalKey.Create("principalKey/$system/user1@spin.com".ToObjectId());

        string data = model.ToJsonPascal();
    }
}
