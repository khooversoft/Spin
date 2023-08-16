using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tokenizer;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools.Validation;

namespace Toolbox.Types.ID;

// user:user1@company3.com
// kid:user1@company3.com/path
// principal-key:user1@company3.com"
// principal-private-key:user1@company3.com"
// tenant:company3.com
// subscription:$system/subscriptionId
// user1@company3.com
internal class ResourceIdTool
{
    public readonly record struct ResourceIdParsed
    {
        public string Id { get; init; }
        public string? Schema { get; init; }
        public string? User { get; init; }
        public string? Domain { get; init; }
        public string? Path { get; init; }
    };

    public static Option<ResourceIdParsed> Parse(string subject)
    {
        var details = InternalParse(subject);
        if (details.IsError()) return details;

        ResourceIdParsed result = details.Return();
        return result;
    }

    private static Option<ResourceIdParsed> InternalParse(string subject)
    {
        if (subject.IsEmpty()) return StatusCode.BadRequest;

        IReadOnlyList<IToken> tokens = new StringTokenizer()
            .UseCollapseWhitespace()
            .Add(":", "@", "/")
            .Parse(Uri.UnescapeDataString(subject));

        Option<ResourceIdParsed> result = new[]
        {
            WithUserAndPaths(subject, tokens),
            WithNoUserAndPaths(subject, tokens),
            UserAndDomain(subject, tokens),
        }.FirstOrDefault(x => x.IsOk(), new Option<ResourceIdParsed>(StatusCode.BadRequest));

        return result;
    }

    private static Option<ResourceIdParsed> WithUserAndPaths(string subject, IReadOnlyList<IToken> tokens) => HasPattern(tokens, hasUserPattern, true) switch
    {
        false => StatusCode.BadRequest,

        // kid:user1@company3.com/path
        // 0  12    34           56
        true => new ResourceIdParsed
        {
            Id = subject,
            Schema = tokens[0].Value,
            User = tokens[2].Value,
            Domain = tokens[4].Value,
            Path = tokens.Skip(6).Aggregate(string.Empty, (a, x) => a + x).ToNullIfEmpty(),
        },
    };

    private static Option<ResourceIdParsed> WithNoUserAndPaths(string subject, IReadOnlyList<IToken> tokens) => HasPattern(tokens, noUserPattern, true) switch
    {
        false => StatusCode.BadRequest,

        // subscription:company3.com/subscriptionId
        // 0           12           34
        true => new ResourceIdParsed
        {
            Id = subject,
            Schema = tokens[0].Value,
            Domain = tokens[2].Value,
            Path = tokens.Skip(4).Aggregate(string.Empty, (a, x) => a + x).ToNullIfEmpty(),
        },
    };

    private static Option<ResourceIdParsed> UserAndDomain(string subject, IReadOnlyList<IToken> tokens) => HasPattern(tokens, onlyUserPattern, false) switch
    {
        false => StatusCode.BadRequest,

        // user1@company3.com
        // 0    12
        true => new ResourceIdParsed
        {
            Id = subject,
            User = tokens[0].Value,
            Domain = tokens[2].Value,
        },
    };

    private static bool HasPattern(IReadOnlyList<IToken> tokens, Func<IToken, bool>[] tests, bool allowPath) =>
        (allowPath ? tokens.Count >= tests.Length : tokens.Count == tests.Length) &&
        tests.Zip(tokens).All(x => x.First(x.Second));

    private static Func<IToken, bool>[] hasUserPattern = new Func<IToken, bool>[]
    {
        x => x.Value.IsNotEmpty(),      // Schema
        x => x.Value == ":",
        x => x.Value.IsNotEmpty(),      // User
        x => x.Value == "@",
        x => x.Value.IsNotEmpty(),      // Domain
    };

    private static Func<IToken, bool>[] noUserPattern = new Func<IToken, bool>[]
    {
        x => x.Value.IsNotEmpty(),      // Schema
        x => x.Value == ":",
        x => x.Value.IsNotEmpty(),      // Domain
    };

    private static Func<IToken, bool>[] onlyUserPattern = new Func<IToken, bool>[]
    {
        x => x.Value.IsNotEmpty(),      // User
        x => x.Value == "@",
        x => x.Value.IsNotEmpty(),      // Domain
    };
}

