using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ResourceIdPathTests
{
    [Theory]
    [InlineData("subscription:name", "$system/$system_name.json")]
    [InlineData("tenant:domain.com", "$system/$system_domain.com.json")]
    [InlineData("userId@domain.com", "domain.com/domain.com_userId@domain.com.json")]
    [InlineData("principal-key:user@domain.com", "domain.com/domain.com_user@domain.com.json")]
    [InlineData("principal-key:user@domain.com/path", "domain.com/user@domain.com/domain.com_user@domain.com_path.json")]
    [InlineData("kid:user1@domain.com/path/path2/path3", "domain.com/user1@domain.com/path/path2/domain.com_user1@domain.com_path_path2_path3.json")]
    [InlineData("contract:domain.com/path", "domain.com/domain.com_path.json")]
    public void TestToPath(string resourceId, string shouldBePath)
    {
        ResourceId id = resourceId;
        string filePath = id.BuildPath();
        filePath.Should().Be(shouldBePath, resourceId);
    }
}
