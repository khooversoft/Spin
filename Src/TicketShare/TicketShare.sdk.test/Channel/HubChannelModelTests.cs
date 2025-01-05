//using FluentAssertions;
//using Toolbox.Extensions;
//using Toolbox.Tools;

//namespace TicketShare.sdk.test.Channel;

//public class HubChannelModelTests
//{
//    [Fact]
//    public void EmptyCompare()
//    {
//        var m1 = new HubChannelRecord();
//        var m2 = new HubChannelRecord();

//        (m1 == m2).Should().BeTrue();
//    }

//    [Fact]
//    public void ChannelMessageRecordEquals()
//    {
//        DateTime dt = DateTime.UtcNow;

//        var m1 = new ChannelMessageRecord
//        {
//            MessageId = "message1",
//            Date = dt,
//            FromPrincipalId = "user1@domain.com",
//            ChannelId = "hub-channel",
//            Message = "message1",
//            Actions = [
//                MessageAction.CreateProposal("proposal1"),
//            ],
//        };

//        var m2 = new ChannelMessageRecord
//        {
//            MessageId = "message1",
//            Date = dt,
//            FromPrincipalId = "user1@domain.com",
//            ChannelId = "hub-channel",
//            Message = "message1",
//            Actions = [
//                MessageAction.CreateProposal("proposal1"),
//            ],
//        };

//        (m1 == m2).Should().BeTrue();

//        var m3 = new ChannelMessageRecord
//        {
//            MessageId = "message2",
//            Date = dt,
//            FromPrincipalId = "user1@domain.com",
//            ChannelId = "hub-channel",
//            Message = "message1",
//            Actions = [
//                MessageAction.CreateProposal("proposal1"),
//            ],
//        };

//        (m1 == m3).Should().BeFalse();
//    }

//    [Fact]
//    public void UserEquals()
//    {
//        const string principalId = "user1@company.com";
//        const string user2PrincipalId = "user2@company.com";
//        var date = DateTime.UtcNow;

//        var m1 = new HubChannelRecord
//        {
//            ChannelId = "company.com/team1",
//            Users = new Dictionary<string, PrincipalChannelRecord>(StringComparer.OrdinalIgnoreCase)
//            {
//                [principalId] = new PrincipalChannelRecord
//                {
//                    PrincipalId = principalId,
//                    Role = ChannelRole.Owner,
//                },
//                [user2PrincipalId] = new PrincipalChannelRecord
//                {
//                    PrincipalId = user2PrincipalId,
//                    Role = ChannelRole.Contributor,
//                    MessageStates = [
//                        new MessageStateRecord { MessageId = "message1", ReadDate = date },
//                    ]
//                }
//            },
//        };

//        var m2 = new HubChannelRecord
//        {
//            ChannelId = "company.com/team1",
//            Users = new Dictionary<string, PrincipalChannelRecord>(StringComparer.OrdinalIgnoreCase)
//            {
//                [principalId] = new PrincipalChannelRecord
//                {
//                    PrincipalId = principalId,
//                    Role = ChannelRole.Owner,
//                },
//                [user2PrincipalId] = new PrincipalChannelRecord
//                {
//                    PrincipalId = user2PrincipalId,
//                    Role = ChannelRole.Contributor,
//                    MessageStates = [
//                        new MessageStateRecord { MessageId = "message1", ReadDate = date },
//                    ]
//                }
//            },
//        };

//        (m1 == m2).Should().BeTrue();
//    }

//    [Fact]
//    public void FullModel()
//    {
//        const string principalId = "user1@company.com";
//        const string user2PrincipalId = "user2@company.com";
//        var date = DateTime.UtcNow;

//        var model = new HubChannelRecord
//        {
//            ChannelId = "company.com/team1",
//            Users = new Dictionary<string, PrincipalChannelRecord>(StringComparer.OrdinalIgnoreCase)
//            {
//                [principalId] = new PrincipalChannelRecord
//                {
//                    PrincipalId = principalId,
//                    Role = ChannelRole.Owner,
//                    MessageStates = [
//                        new MessageStateRecord { MessageId = "message1", ReadDate = DateTime.UtcNow },
//                    ]
//                },
//                [user2PrincipalId] = new PrincipalChannelRecord
//                {
//                    PrincipalId = user2PrincipalId,
//                    Role = ChannelRole.Contributor,
//                    MessageStates = [
//                        new MessageStateRecord { MessageId = "message1", ReadDate = DateTime.UtcNow },
//                    ]
//                }
//            },
//            Messages = [
//                new ChannelMessageRecord
//                {
//                    MessageId = "message1",
//                    Date = date,
//                    FromPrincipalId = user2PrincipalId,
//                    ChannelId = "company.com/team1",
//                    Message = "hello",
//                    Actions =[
//                        MessageAction.CreateProposal("proposal1")
//                        ],
//                },
//                new ChannelMessageRecord
//                {
//                    MessageId = "message2",
//                    Date = date,
//                    FromPrincipalId = user2PrincipalId,
//                    ChannelId = "company.com/team1",
//                    Message = "hello2",
//                },
//                new ChannelMessageRecord
//                {
//                    MessageId = "message3",
//                    Date = date,
//                    FromPrincipalId = "user1@company.com",
//                    ChannelId = "company.com/team1",
//                    Message = "hello from owner",
//                }
//            ],
//        };

//        var json = model.ToJson();

//        var readModel = json.ToObject<HubChannelRecord>().NotNull();
//        (model == readModel).Should().BeTrue();

//        readModel.GetMessages(user2PrincipalId).Action(x =>
//        {
//            x.Count.Should().Be(3);
//            x.Select(y => y.Message.MessageId).OrderBy(x => x).Should().Equal(["message1", "message2", "message3"]);
//            x.Where(x => x.ReadDate != null).Select(y => y.Message.MessageId).Should().Equal(["message1"]);
//        });

//        // Mark message read
//        var updateModel = model.ToBuilder().MarkRead(user2PrincipalId, "message2", DateTime.UtcNow.AddDays(1)).Build();
//        updateModel.GetMessages(user2PrincipalId).Action(x =>
//        {
//            x.Count.Should().Be(3);
//            x.Select(y => y.Message.MessageId).OrderBy(x => x).Should().Equal(["message1", "message2", "message3"]);
//            x.Where(x => x.ReadDate != null).Select(y => y.Message.MessageId).OrderBy(x => x).Should().Equal(["message1", "message2"]);
//        });

//        // Mark same message read, should not change anything
//        updateModel = updateModel.ToBuilder().MarkRead(user2PrincipalId, "message2", DateTime.UtcNow.AddDays(1)).Build();
//        updateModel.GetMessages(user2PrincipalId).Action(x =>
//        {
//            x.Count.Should().Be(3);
//            x.Select(y => y.Message.MessageId).OrderBy(x => x).Should().Equal(["message1", "message2", "message3"]);
//            x.Where(x => x.ReadDate != null).Select(y => y.Message.MessageId).OrderBy(x => x).Should().Equal(["message1", "message2"]);
//        });

//        updateModel = updateModel.ToBuilder().AddMessage(new ChannelMessageRecord
//        {
//            MessageId = "message4",
//            FromPrincipalId = user2PrincipalId,
//            ChannelId = "company.com/team1",
//            Message = "hello4",
//        }).Build();

//        updateModel.GetMessages(user2PrincipalId).Action(x =>
//        {
//            x.Count.Should().Be(4);
//            x.Select(y => y.Message.MessageId).OrderBy(x => x).Should().Equal(["message1", "message2", "message3", "message4"]);
//            x.Where(x => x.ReadDate != null).Select(y => y.Message.MessageId).OrderBy(x => x).Should().Equal(["message1", "message2"]);
//        });

//        updateModel = updateModel.ToBuilder().MarkRead(user2PrincipalId, "message4", DateTime.UtcNow.AddDays(1)).Build();
//        updateModel.GetMessages(user2PrincipalId).Action(x =>
//        {
//            x.Count.Should().Be(4);
//            x.Select(y => y.Message.MessageId).OrderBy(x => x).Should().Equal(["message1", "message2", "message3", "message4"]);
//            x.Where(x => x.ReadDate != null).Select(y => y.Message.MessageId).OrderBy(x => x).Should().Equal(["message1", "message2", "message4"]);
//        });
//    }
//}
