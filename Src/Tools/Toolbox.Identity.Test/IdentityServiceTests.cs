using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Identity.Test;

public class IdentityServiceTests
{
    [Fact]
    public async Task AddIdentity()
    {
        var context = new ScopeContext(NullLogger<ScopeContext>.Instance);
        var store = new IdentityInMemoryStore(NullLogger<IdentityInMemoryStore>.Instance);
        var service = new IdentityService(store, NullLogger<IdentityService>.Instance);

        var userIdentity = new PrincipalIdentity
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "userName1",
            Email = "userName1@domain.com",
        };

        userIdentity.Validate().IsOk().Should().BeTrue();

        var addResult = await service.Set(userIdentity, null, context);
        addResult.IsOk().Should().BeTrue();

        var idLookupOption = await service.GetById(userIdentity.Id, context);
        idLookupOption.IsOk().Should().BeTrue();

        var userNameOption = await service.GetByUserName(userIdentity.UserName, context);
        userNameOption.IsOk().Should().BeTrue();

        var emailLookupOption = await service.GetByEmail(userIdentity.Email, context);
        emailLookupOption.IsOk().Should().BeTrue();

        var deleteResult = await service.Delete(userIdentity.Id, context);
        deleteResult.IsOk().Should().BeTrue(deleteResult.ToString());
    }

    [Fact]
    public async Task AddTwoIdentity()
    {
        var context = new ScopeContext(NullLogger<ScopeContext>.Instance);
        var store = new IdentityInMemoryStore(NullLogger<IdentityInMemoryStore>.Instance);
        var service = new IdentityService(store, NullLogger<IdentityService>.Instance);

        const string userName1 = "userName1";
        const string userName2 = "userName2";

        await addIdentity(userName1, "userName1@domain.com");
        await addIdentity(userName2, "userName2@domain.com");


        (await service.GetByUserName(userName1, context)).Action(async x =>
        {
            x.IsOk().Should().BeTrue();
            var deleteResult = await service.Delete(x.Return().Id, context);
            deleteResult.IsOk().Should().BeTrue();
        });

        (await service.GetByUserName(userName2, context)).Action(async x =>
        {
            x.IsOk().Should().BeTrue();
            var deleteResult = await service.Delete(x.Return().Id, context);
            deleteResult.IsOk().Should().BeTrue();
        });

        async Task addIdentity(string userName, string email)
        {
            var userIdentity = new PrincipalIdentity
            {
                Id = Guid.NewGuid().ToString(),
                UserName = userName,
                Email = email,
            };

            userIdentity.Validate().IsOk().Should().BeTrue();

            var addResult = await service.Set(userIdentity, null, context);
            addResult.IsOk().Should().BeTrue();

            var idLookupOption = await service.GetById(userIdentity.Id, context);
            idLookupOption.IsOk().Should().BeTrue();

            var userNameOption = await service.GetByUserName(userIdentity.UserName, context);
            userNameOption.IsOk().Should().BeTrue();

            var emailLookupOption = await service.GetByEmail(userIdentity.Email, context);
            emailLookupOption.IsOk().Should().BeTrue();
        }
    }
}