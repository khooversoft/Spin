using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Types;

namespace Toolbox.Identity.Test;

public class IdentityServiceTests
{
    [Fact]
    public void AddIdentity()
    {
        var context = new ScopeContext(NullLogger<ScopeContext>.Instance);
        var store = new IdentityInMemoryStore(NullLogger<IdentityInMemoryStore>.Instance);
        var service = new IdentityService(store, NullLogger<IdentityService>.Instance);

        var userIdentity = new IdentityUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "userName1",
            Email = "userName1@domain.com",
        };

        userIdentity.Validate().IsOk().Should().BeTrue();

        var addResult = service.Add(userIdentity, );
    }
}