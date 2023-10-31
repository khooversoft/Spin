using Toolbox.Extensions;
using Toolbox.LangTools;

namespace Toolbox.Types.ID;


internal class ResourceIdTool
{
    private enum TokenType
    {
        None,
        Syntax,
        Schema,
        SystemName,
        User,
        Domain,
        Path,
        Colon,
        AtSign,
    }

    private readonly record struct TokenResult
    {
        public TokenType Type { get; init; }
        public string? Value { get; init; }
        public static TokenResult None { get; } = new TokenResult { Type = TokenType.None };
    }

    private readonly record struct Test
    {
        public Func<IToken, TokenResult>[] Pattern { get; init; }
        public Func<IReadOnlyList<IToken>, bool> PostTest { get; init; }
        public Func<string, TokenResult[], IReadOnlyList<IToken>, ResourceId> Build { get; init; }
    }

    public static Option<ResourceId> Parse(string subject)
    {
        if (subject.IsEmpty()) return StatusCode.BadRequest;

        IReadOnlyList<IToken> tokens = new StringTokenizer()
            .UseCollapseWhitespace()
            .Add(":", "@", "/")
            .Parse(Uri.UnescapeDataString(subject));

        var tests = new[]
        {
            SystemTest,
            TenantTest,
            PrincipalTest,
            OwnedTest,
            AccountTest
        };

        Option<ResourceId> result = tests
            .Select(x => isMatch(tokens, subject, x))
            .SkipWhile(x => x.IsError())
            .FirstOrDefault(StatusCode.NotFound);

        return result;

        Option<ResourceId> isMatch(IReadOnlyList<IToken> tokens, string id, Test test)
        {
            TokenResult[] result = test.Pattern.Zip(tokens).Select(x => x.First(x.Second)).ToArray();
            if (result.Any(x => x == TokenResult.None)) return StatusCode.NotFound;

            if (!test.PostTest(tokens)) return StatusCode.NotFound;

            return test.Build(id, result, tokens);
        }
    }

    // {schema}:{systemName}
    private static Test SystemTest = new Test
    {
        Pattern = new Func<IToken, TokenResult>[]
        {
            x => IdPatterns.IsName(x.Value) ? new TokenResult { Type = TokenType.Schema, Value = x.Value } : TokenResult.None,
            x => x.Value == ":" ? new TokenResult { Type = TokenType.Syntax } : TokenResult.None,
            x => IdPatterns.IsName(x.Value) ? new TokenResult { Type = TokenType.SystemName, Value = x.Value } : TokenResult.None,
        },
        PostTest = x => x.Count == 3,
        Build = (id, r, tokens) => new ResourceId
        {
            Id = id,
            Type = ResourceType.System,
            Schema = r.First(x => x.Type == TokenType.Schema).Value,
            SystemName = r.First(x => x.Type == TokenType.SystemName).Value,
        },
    };

    // {schema}:{domain}
    private static Test TenantTest = new Test
    {
        Pattern = new Func<IToken, TokenResult>[]
        {
            x => IdPatterns.IsName(x.Value) ? new TokenResult { Type = TokenType.Schema, Value = x.Value } : TokenResult.None,
            x => x.Value == ":" ? new TokenResult { Type = TokenType.Syntax } : TokenResult.None,
            x => IdPatterns.IsDomain(x.Value) ? new TokenResult { Type = TokenType.Domain, Value = x.Value } : TokenResult.None,
        },
        PostTest = x => x.Count == 3,
        Build = (id, r, tokens) => new ResourceId
        {
            Id = id,
            Type = ResourceType.Tenant,
            Schema = r.First(x => x.Type == TokenType.Schema).Value,
            Domain = r.First(x => x.Type == TokenType.Domain).Value,
        },
    };

    // {user}@{domain}
    private static Test PrincipalTest = new Test
    {
        Pattern = new Func<IToken, TokenResult>[]
        {
            x => IdPatterns.IsName(x.Value) ? new TokenResult { Type = TokenType.User, Value = x.Value } : TokenResult.None,
            x => x.Value == "@" ? new TokenResult { Type = TokenType.Syntax } : TokenResult.None,
            x => IdPatterns.IsDomain(x.Value) ? new TokenResult { Type = TokenType.Domain, Value = x.Value } : TokenResult.None,
        },
        PostTest = x => x.Count == 3,
        Build = (id, r, tokens) => new ResourceId
        {
            Id = id,
            Type = ResourceType.Principal,
            User = r.First(x => x.Type == TokenType.User).Value,
            Domain = r.First(x => x.Type == TokenType.Domain).Value,
        }.Func(x => x with
        {
            PrincipalId = $"{x.User}@{x.Domain}"
        }),
    };

    // {schema}:{user}@{domain}[/{path}...}]
    private static Test OwnedTest = new Test
    {
        Pattern = new Func<IToken, TokenResult>[]
        {
            x => IdPatterns.IsName(x.Value) ? new TokenResult { Type = TokenType.Schema, Value = x.Value } : TokenResult.None,
            x => x.Value == ":" ? new TokenResult { Type = TokenType.Syntax } : TokenResult.None,
            x => IdPatterns.IsName(x.Value) ? new TokenResult { Type = TokenType.User, Value = x.Value } : TokenResult.None,
            x => x.Value == "@" ? new TokenResult { Type = TokenType.Syntax } : TokenResult.None,
            x => IdPatterns.IsDomain(x.Value) ? new TokenResult { Type = TokenType.Domain, Value = x.Value } : TokenResult.None,
        },
        PostTest = x => x.Count >= 5 && x.Skip(6).All(y => y.Value == "/" || IdPatterns.IsPath(y.Value)),
        Build = (id, r, tokens) => new ResourceId
        {
            Id = id,
            Type = ResourceType.Owned,
            Schema = r.First(x => x.Type == TokenType.Schema).Value,
            User = r.First(x => x.Type == TokenType.User).Value,
            Domain = r.First(x => x.Type == TokenType.Domain).Value,
            Path = tokens.Skip(6).Aggregate(string.Empty, (a, x) => a + x).ToNullIfEmpty(),
        }.Func(x => x with
        {
            PrincipalId = $"{x.User}@{x.Domain}",
            AccountId = x.Path.IsNotEmpty() ? $"{x.Domain}/{x.Path}" : null,
        }),
    };

    // {schema}:{domain}/{path}[/{path}...}]
    private static Test AccountTest = new Test
    {
        Pattern = new Func<IToken, TokenResult>[]
        {
            x => IdPatterns.IsName(x.Value) ? new TokenResult { Type = TokenType.Schema, Value = x.Value } : TokenResult.None,
            x => x.Value == ":" ? new TokenResult { Type = TokenType.Syntax } : TokenResult.None,
            x => IdPatterns.IsDomain(x.Value) ? new TokenResult { Type = TokenType.Domain, Value = x.Value } : TokenResult.None,
        },
        PostTest = x => x.Count > 3 && x.Skip(4).All(y => y.Value == "/" || IdPatterns.IsPath(y.Value)),
        Build = (id, r, tokens) => new ResourceId
        {
            Id = id,
            Type = ResourceType.DomainOwned,
            Schema = r.First(x => x.Type == TokenType.Schema).Value,
            Domain = r.First(x => x.Type == TokenType.Domain).Value,
            Path = tokens.Skip(4).Aggregate(string.Empty, (a, x) => a + x).ToNullIfEmpty(),
        }.Func(x => x with
        {
            AccountId = $"{x.Domain}/{x.Path}"
        }),
    };
}
