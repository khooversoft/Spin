using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk.test.Directory;

internal static class DirectoryData
{
    public static IReadOnlyList<ArticleManifest> TestArticleManifests = new List<ArticleManifest>()
    {
        new ArticleManifest
        {
            ArticleId = "Blog/Csharp/Option.manifest.json",
            Title = "Generalized Execution State",
            Author = "khoover",
            CreatedDate = DateTime.Parse("2023-07-11T00:00:00.0000000"),
            Commands = new []
            {
                "[summary] Blog/Csharp/Option.summary.md = Option.summary.md",
                "[main] Blog/Csharp/Option.doc.md = Option.doc.md",
            },
            Tags = "db=resume;Language=C#;Area=Tools;Design=Framework",
        },
        new ArticleManifest
        {
            ArticleId = "Blog/Csharp/Tuple.manifest.json",
            Title = "Tuples",
            Author = "khoover",
            CreatedDate = DateTime.Parse("2023-07-21T00:00:00.0000000"),
            Commands = new []
            {
                "[summary] Blog/Csharp/Tuple.summary.md = Tuple.summary.md",
                "[main] Blog/Csharp/Tuple.doc.md = Tuple.doc.md",
            },
            Tags = "db=resume;Language=C#;Area=Language;Design=Functional",
        },
        new ArticleManifest
        {
            ArticleId = "Blog/Design/Functional-Programming.manifest.json",
            Title = "Functional Programming",
            Author = "khoover",
            CreatedDate = DateTime.Parse("2023-07-01T00:00:00.0000000"),
            Commands = new []
            {
                "[summary] Blog/Design/Functional-Programming.summary.md = Functional-Programming.summary.md",
                "[main] Blog/Design/Functional-Programming.doc.md = Functional-Programming.doc.md",
            },
            Tags = "db=article;Area=Strategy;Design=Functional",
        },
        new ArticleManifest
        {
            ArticleId = "Blog/Toolbox/ScopeContext.manifest.json",
            Title = "Scope Execution Context",
            Author = "khoover",
            CreatedDate = DateTime.Parse("2023-07-31T00:00:00.0000000"),
            Commands = new []
            {
                "[summary] Blog/Toolbox/ScopeContext.summary.md = ScopeContext.summary.md",
                "[main] Blog/Toolbox/ScopeContext.doc.md = ScopeContext.doc.md",
            },
            Tags = "db=resume;Language=C#;Area=Tools;Design=Framework",
        },
        new ArticleManifest
        {
            ArticleId = "Blog/Tools/PackagesUsed.manifest.json",
            Title = "Framework, servers, and Nuget used",
            Author = "khoover",
            CreatedDate = DateTime.Parse("2023-05-02T00:00:00.0000000"),
            Commands = new []
            {
                "[main] Blog/Tools/PackagesUsed.doc.md = PackagesUsed.doc.md",
            },
            Tags = "noSummary;db=article;Area=Tools",
        },
        new ArticleManifest
        {
            ArticleId = "Blog/Design/Composition/Composition.manifest.json",
            Title = "Inheritance and Composition",
            Author = "khoover",
            CreatedDate = DateTime.Parse("2023-05-22T00:00:00.0000000"),
            Commands = new []
            {
                "[summary] Blog/Design/Composition/Composition.summary.md = Composition.summary.md",
                "[main] Blog/Design/Composition/Composition.doc.md = Composition.doc.md",
            },
            Tags = "db=article;Area=Strategy;Design=Functional",
        },
        new ArticleManifest
        {
            ArticleId = "Blog/Design/FrameworkDesign/Coupling.manifest.json",
            Title = "Coupling",
            Author = "khoover",
            CreatedDate = DateTime.Parse("2023-06-11T00:00:00.0000000"),
            Commands = new []
            {
                "[summary] Blog/Design/FrameworkDesign/Coupling.summary.md = Coupling.summary.md",
                "[main] Blog/Design/FrameworkDesign/Coupling.doc.md = Coupling.doc.md",
            },
            Tags = "db=article;Area=Strategy;Design=Refactoring",
        },
        new ArticleManifest
        {
            ArticleId = "Blog/Design/FrameworkDesign/FrameworkDesign.manifest.json",
            Title = "Framework Design Rules and Considerations",
            Author = "khoover",
            CreatedDate = DateTime.Parse("2023-05-12T00:00:00.0000000"),
            Commands = new []
            {
                "[summary] Blog/Design/FrameworkDesign/FrameworkDesign.summary.md = FrameworkDesign.summary.md",
                "[main] Blog/Design/FrameworkDesign/FrameworkDesign.doc.md = FrameworkDesign.doc.md",
            },
            Tags = "db=article;Area=Strategy;Design=Framework",
        },
        new ArticleManifest
        {
            ArticleId = "Blog/Design/FrameworkDesign/SideEffects.manifest.json",
            Title = "Side Effects",
            Author = "khoover",
            CreatedDate = DateTime.Parse("2023-06-01T00:00:00.0000000"),
            Commands = new []
            {
                "[summary] Blog/Design/FrameworkDesign/SideEffects.summary.md = SideEffects.summary.md",
                "[main] Blog/Design/FrameworkDesign/SideEffects.doc.md = SideEffects.doc.md",
            },
            Tags = "db=article;Area=Strategy;Design=Functional",
        },
        new ArticleManifest
        {
            ArticleId = "Blog/Design/FrameworkDesign/Testability.manifest.json",
            Title = "Testability",
            Author = "khoover",
            CreatedDate = DateTime.Parse("2023-06-21T00:00:00.0000000"),
            Commands = new []
            {
                "[summary] Blog/Design/FrameworkDesign/Testability.summary.md = Testability.summary.md",
                "[main] Blog/Design/FrameworkDesign/Testability.doc.md = Testability.doc.md",
            },
            Tags = "db=resume;Area=Strategy;Design=Framework",
        },
    };

    public static IDirectoryActor Load()
    {
        Option<GraphMap> map = new ArticleDirectoryBuilder()
            .Add(TestArticleManifests)
            .Build();
        map.IsOk().Should().BeTrue(map.ToString());

        return new DirectoryFake(map.Return());
    }
}

public class DirectoryFake : IDirectoryActor
{
    public DirectoryFake(GraphMap map) => Map = map.NotNull();
    public GraphMap Map { get; }

    public Task<Option> Clear(string principalId, string traceId) => throw new NotImplementedException();

    public Task<Option<GraphCommandResults>> Execute(string command, string _)
    {
        GraphCommandExceuteResults result = Map.Command().Execute(command).ThrowOnError().Return();
        return result.ConvertTo().ToOption().ToTaskResult();
    }
}

