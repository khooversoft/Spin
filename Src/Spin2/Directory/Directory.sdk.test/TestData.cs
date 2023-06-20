using Directory.sdk.Models;
using Toolbox.Extensions;

namespace Directory.sdk.test;

public class TestData
{
    [Fact]
    public void CreateIdentityData()
    {
        var principle = new UserPrincipal
        {
            UserId = "user:$system/user1@tenant.com",
            DisplayName = "user 1",
            FirstName = "user1",
            LastName = "user1-last",
            Email = "user1@tenant.com",

            Phone = new[]
            {
                new UserPhone { Type = "home", Number= "252-555-1200" },
                new UserPhone { Type = "work", Number="252-555-1201" },
            }.ToArray(),

            Addresses = new[]
            {
                new UserAddress{ Type = "home", Address1 = "line1", City = "city1", State = "state1", Country = "USA", ZipCode = "39393" }
            }.ToArray(),
        };

        string data = principle.ToJsonPascal();
    }
}
