using SpinCluster.sdk.Actors.Key;
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
