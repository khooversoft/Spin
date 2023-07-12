using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Security;

public class HashTests
{
    [Fact]
    public void GivenString_WhenHashed_Match()
    {
        string v1 = "Hello";
        string v2 = "Hello";

        string h1 = v1.ToBytes().ToSHA256Hash();
        string h2 = v2.ToBytes().ToSHA256Hash();

        (h1 == h2).Assert(x => x == true, "Hashes do not match");

        string v3 = "hello1";
        string h3 = v3.ToBytes().ToSHA256Hash();

        (h1 != h3).Assert(x => x == true, "Hashes match, when different");
    }
}
