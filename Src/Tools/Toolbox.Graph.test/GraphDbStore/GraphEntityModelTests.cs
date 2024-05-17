//using FluentAssertions;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph.test.GraphDbStore;

//public class GraphEntityModelTests
//{
//    [Fact]
//    public void EntityGraphPropertiesNoId()
//    {
//        var entity = new TestEntity
//        {
//            UserName = "User name",
//            Email = "user@domain.com",
//            EmailConfirmed = true,
//            PasswordHash = "passwordHash",
//            Name = "name1-user001",
//            LoginProvider = "microsoft",
//            ProviderKey = "user001-microsoft-id",
//        };

//        var result = TestEntity.GraphSchema.CreateAddCommands(entity);
//        result.IsOk().Should().BeTrue(result.ToString());
//    }

//    //[Fact]
//    //public void EntityGraphProperties()
//    //{
//    //    var entity = new TestEntity
//    //    {
//    //        Id = "user001",
//    //        UserName = "User name",
//    //        Email = "user@domain.com",
//    //        EmailConfirmed = true,
//    //        PasswordHash = "passwordHash",
//    //        Name = "name1-user001",
//    //        LoginProvider = "microsoft",
//    //        ProviderKey = "user001-microsoft-id",
//    //    };

//    //    var commandOptions = entity.GetGraphCommands();
//    //    commandOptions.IsOk().Should().BeTrue();

//    //    var nodeKeys = new string[]
//    //    {
//    //        "logonProvider:microsoft/user001-microsoft-id",
//    //        "user:user001",
//    //    };

//    //    var commands = commandOptions.Return();

//    //    commands
//    //        .OfType<NodeCreateCommand>()
//    //        .Select(x => x.NodeKey)
//    //        .OrderBy(x => x)
//    //        .Zip(nodeKeys, (o, i) => (o, i))
//    //        .Select(x => (x, pass: x.o == x.i))
//    //        .All(x => x.pass).Should().BeTrue();

//    //    commands.Select(x => x.GetAddCommand()).ToArray().Action(x =>
//    //    {
//    //        string base64 = entity.ToJson64();

//    //        x.Length.Should().Be(3);
//    //        x[0].Should().Be($"upsert node key=user:user001, userEmail=user@domain.com,Name=name1-user001, entity {{ '{base64}' }};");
//    //        x[1].Should().Be("upsert node key=logonProvider:microsoft/user001-microsoft-id, uniqueIndex;");
//    //        x[2].Should().Be("add unique edge fromKey=logonProvider:microsoft/user001-microsoft-id, toKey=user:user001, edgeType=uniqueIndex;");
//    //    });
//    //}

//    //[Fact]
//    //public void EntityGraphProperties2()
//    //{
//    //    var entity = new TestEntity
//    //    {
//    //        Id = "user001",
//    //        UserName = "User name",
//    //        Email = "user@domain.com",
//    //        NormalizedUserName = "user001-normalized",
//    //        EmailConfirmed = true,
//    //        PasswordHash = "passwordHash",
//    //        Name = "name1-user001",
//    //        LoginProvider = "microsoft",
//    //        ProviderKey = "user001-microsoft-id",
//    //    };

//    //    var commandOptions = entity.GetGraphCommands();
//    //    commandOptions.IsOk().Should().BeTrue();

//    //    var nodeKeys = new string[]
//    //    {
//    //        "logonProvider:microsoft/user001-microsoft-id",
//    //        "user:user001",
//    //        "userNormalizedUserName:user001-normalized",
//    //    };

//    //    var commands = commandOptions.Return();

//    //    commands
//    //        .OfType<NodeCreateCommand>()
//    //        .Select(x => x.NodeKey)
//    //        .OrderBy(x => x)
//    //        .Zip(nodeKeys, (o, i) => (o, i))
//    //        .Select(x => (x, pass: x.o == x.i))
//    //        .All(x => x.pass).Should().BeTrue();

//    //    commands.Select(x => x.GetAddCommand()).ToArray().Action(x =>
//    //    {
//    //        string base64 = entity.ToJson64();

//    //        x.Length.Should().Be(5);
//    //        x[0].Should().Be($"upsert node key=user:user001, userEmail=user@domain.com,Name=name1-user001, entity {{ '{base64}' }};");
//    //        x[1].Should().Be("upsert node key=userNormalizedUserName:user001-normalized, uniqueIndex;");
//    //        x[2].Should().Be("add unique edge fromKey=userNormalizedUserName:user001-normalized, toKey=user:user001, edgeType=uniqueIndex;");
//    //        x[3].Should().Be("upsert node key=logonProvider:microsoft/user001-microsoft-id, uniqueIndex;");
//    //        x[4].Should().Be("add unique edge fromKey=logonProvider:microsoft/user001-microsoft-id, toKey=user:user001, edgeType=uniqueIndex;");
//    //    });
//    //}

//    //[Fact]
//    //public void IndexPropertiesAllPropertiesMustHaveValue()
//    //{
//    //    var entity = new TestEntity
//    //    {
//    //        Id = "user001",
//    //        UserName = "User name",
//    //        Email = "user@domain.com",
//    //        NormalizedUserName = "user001-normalized",
//    //        EmailConfirmed = true,
//    //        PasswordHash = "passwordHash",
//    //        Name = "name1-user001",
//    //        LoginProvider = "microsoft",
//    //        //ProviderKey = "user001-microsoft-id",
//    //    };

//    //    var result = entity.GetGraphCommands();
//    //    result.IsOk().Should().BeTrue();
//    //    result.Return().Length.Should().Be(3);
//    //}

//    //[Fact]
//    //public void PropertyNameIncorrect()
//    //{
//    //    var entity = new TestEntity2
//    //    {
//    //        Id = "user001",
//    //        UserName = "User name",
//    //        Email = "user@domain.com",
//    //        NormalizedUserName = "user001-normalized",
//    //        EmailConfirmed = true,
//    //        PasswordHash = "passwordHash",
//    //        Name = "name1-user001",
//    //        LoginProvider = "microsoft",
//    //        ProviderKey = "user001-microsoft-id",
//    //    };

//    //    Action a = () => entity.GetGraphCommands();
//    //    a.Should().Throw<ArgumentException>();
//    //}

//    private sealed record TestEntity
//    {
//        //[GraphKey("user")]
//        public string Id { get; set; } = null!;

//        public string UserName { get; set; } = null!;

//        //[GraphTag("userEmail")]   // userEmail=user@domain.com
//        public string Email { get; set; } = null!;

//        //[GraphNodeIndex("userNormalizedUserName")]  // nodeKey = "userNormalizedUserName:user001" -> unique edge to "user:user001"
//        public string NormalizedUserName { get; set; } = null!;

//        public bool EmailConfirmed { get; set; }
//        public string PasswordHash { get; set; } = null!;

//        //[GraphTag()]   // Name=name1-user001
//        public string Name { get; set; } = null!;

//        //[GraphNodeIndex("logonProvider", Format = "{LoginProvider}/{ProviderKey}")]  // nodeKey="logonProvider:microsoft/user001-microsoft-id" -> unique edge to "user:user001"
//        public string LoginProvider { get; set; } = null!;
//        public string ProviderKey { get; set; } = null!;

//        public static IGraphSchema<TestEntity> GraphSchema { get; } = new GraphSchema<TestEntity>()
//            .Node(x => OptionTool.OptionSwitch<string>(x.Id.IsNotEmpty(), () => $"user:{x.Id.NotEmpty()}"))
//            .Index("userName", x => OptionTool.OptionSwitch<string>(x.UserName.IsNotEmpty(), () => $"user:{x.UserName.NotEmpty()}"))
//            .Index("email", x => OptionTool.OptionSwitch<string>(x.Email.IsNotEmpty(), () => $"user:{x.Email.NotEmpty()}"))
//            .Tags("emailTag", x => OptionTool.OptionSwitch<string>(x.Email.IsNotEmpty(), () => $"user:{x.Email.NotEmpty()}"))
//            .Index("logonProvider", x => $"logonProvider:{x.LoginProvider.NotEmpty().ToLower() + "/" + x.ProviderKey.NotEmpty().ToLower()}" : StatusCode.BadRequest)
//            .Build();

//        private static Option<string> Format(string? value, Func<TestEntity, Option<string>> func) => value.IsEmpty() ? StatusCode.BadRequest : func(this);
//    }

//    //private sealed record TestEntity2
//    //{
//    //    [GraphKey("user:{Id}")]
//    //    public string Id { get; set; } = null!;

//    //    public string UserName { get; set; } = null!;

//    //    [GraphTag("userEmail")]   // userEmail=user@domain.com
//    //    public string Email { get; set; } = null!;

//    //    [GraphNodeIndex("userNormalizedUserName")]  // nodeKey = "userNormalizedUserName:user001" -> unique edge to "user:user001"
//    //    public string NormalizedUserName { get; set; } = null!;

//    //    public bool EmailConfirmed { get; set; }
//    //    public string PasswordHash { get; set; } = null!;
//    //    public string Name { get; set; } = null!;

//    //    [GraphNodeIndex("logonProvider", Format = "{LoginProvider}/{x-ProviderKey}")]  // nodeKey="logonProvider:microsoft/user001-microsoft-id" -> unique edge to "user:user001"
//    //    public string LoginProvider { get; set; } = null!;
//    //    public string ProviderKey { get; set; } = null!;
//    //}
//}
