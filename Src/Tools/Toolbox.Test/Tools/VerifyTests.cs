using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.Test.Tools;

public class VerifyTests
{
    [Fact]
    public void GivenNotNull_VerifyNotNull_ShouldPass()
    {
        List<string>? kv = new List<string>();
        kv.NotNull();

        kv = null;
        kv.NotNull();
    }
}
