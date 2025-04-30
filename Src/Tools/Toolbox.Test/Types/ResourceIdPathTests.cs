using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ResourceIdPathTests
{
    [Theory]
    [InlineData("subscription:name", "subscription/subscription_name.json")]
    [InlineData("tenant:domain.com", "tenant/tenant_domain.com.json")]
    [InlineData("userId@domain.com", "user/domain.com/user_domain.com_userId@domain.com.json")]
    [InlineData("principal-key:user@domain.com", "principal-key/domain.com/principal-key_domain.com_user@domain.com.json")]
    [InlineData("principal-key:user@domain.com/path", "principal-key/domain.com/user@domain.com/principal-key_domain.com_user@domain.com_path.json")]
    [InlineData("kid:user1@domain.com/path/path2/path3", "kid/domain.com/user1@domain.com/path/path2/kid_domain.com_user1@domain.com_path_path2_path3.json")]
    [InlineData("contract:domain.com/path", "contract/domain.com/contract_domain.com_path.json")]
    public void TestToPath(string resourceId, string shouldBePath)
    {
        ResourceId id = resourceId;
        string filePath = id.BuildPathWithExtension();
        filePath.Be(shouldBePath, resourceId);
    }
}
