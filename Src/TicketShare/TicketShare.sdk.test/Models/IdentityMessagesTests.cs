//using FluentAssertions;
//using Microsoft.Extensions.DependencyInjection;
//using TicketShare.sdk.Applications;
//using Toolbox.Extensions;
//using Toolbox.Types;

//namespace TicketShare.sdk.test.Models;

//public class IdentityMessagesTests
//{
//    [Fact]
//    public async Task FullLifeCycle()
//    {
//        var testHost = new TicketShareTestHost();
//        var client = testHost.ServiceProvider.GetRequiredService<IdentityMessagesClient>();
//        var context = testHost.GetScopeContext<IdentityMessagesTests>();
//        const string principalId = "user1@domain.com";
//        const string sendPrincipalId = "user2@Domain.com";
//        const string userName = "user1";

//        var accountRecord = TestTool.Create(principalId);
//        await TestTool.AddIdentityUser(accountRecord.PrincipalId, userName, testHost, context);

//        var msgContainer = new IdentityMessagesRecord
//        {
//            PrincipalId = principalId,
//        };

//        var writeOption = await client.Add(msgContainer, context);
//        writeOption.IsOk().Should().BeTrue();

//        var readOption = await client.Get(principalId, context);
//        readOption.IsOk().Should().BeTrue();
//        (msgContainer == readOption.Return()).Should().BeTrue();

//        (await client.Messages.GetMessages(principalId, false, context)).Action(x =>
//        {
//            x.IsOk().Should().BeTrue();
//            x.Return().Count.Should().Be(0);
//        });

//        var sendOption = await client.Messages.Send(principalId, sendPrincipalId, "Test message", null, context);
//        sendOption.IsOk().Should().BeTrue();
//        string messageId = sendOption.Return();

//        (await client.Messages.GetMessages(principalId, false, context)).Action(x =>
//        {
//            x.IsOk().Should().BeTrue();
//            x.Return().Action(y =>
//            {
//                y.Count.Should().Be(1);
//                y[0].ToPrincipalId.Should().Be(principalId);
//                y[0].FromPrincipalId.Should().Be(sendPrincipalId);
//                y[0].Message.Should().Be("Test message");
//                y[0].ProposalId.Should().BeNull();
//                y[0].ReadDate.Should().BeNull();
//            });
//        });

//        (await client.Messages.MarkRead(principalId, messageId, true, context)).IsOk().Should().BeTrue();

//        (await client.Messages.GetMessages(principalId, false, context)).Action(x =>
//        {
//            x.IsOk().Should().BeTrue();
//            x.Return().Action(y =>
//            {
//                y.Count.Should().Be(1);
//                y[0].ToPrincipalId.Should().Be(principalId);
//                y[0].FromPrincipalId.Should().Be(sendPrincipalId);
//                y[0].Message.Should().Be("Test message");
//                y[0].ProposalId.Should().BeNull();
//                y[0].ReadDate.Should().NotBeNull();
//            });
//        });

//        (await client.Messages.Remove(principalId, messageId, context)).IsOk().Should().BeTrue();

//        (await client.Messages.GetMessages(principalId, false, context)).Action(x =>
//        {
//            x.IsOk().Should().BeTrue();
//            x.Return().Count.Should().Be(0);
//        });

//        var deleteOption = await client.Delete(principalId, context);
//        deleteOption.IsOk().Should().BeTrue();

//        var result = await client.Get(principalId, context);
//        result.IsNotFound().Should().BeTrue();
//    }
//}
