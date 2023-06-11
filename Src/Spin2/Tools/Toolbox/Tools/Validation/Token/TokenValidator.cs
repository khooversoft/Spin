using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Types.Maybe;

namespace Toolbox.Tools.Validation.Token;

public enum TokenValidatorType
{
    First,
    Last,
    Middle,
    All,
}

public class TokenValidator
{
    public IList<ITokenValidatorTest> Tests { get; } = new List<ITokenValidatorTest>();
    public TokenValidator Add(ITokenValidatorTest test) => this.Action(x => x.Tests.Add(test));

    public bool Validate(string? subject)
    {
        if (subject.IsEmpty()) return Tests.Count == 0 ? true : false;

        return Tests
            .GroupBy(x => x.ValidatorType)
            .Select(x => x.Any(y => testType(y)))
            .All(x => x == true);

        bool testType(ITokenValidatorTest test) => test.ValidatorType switch
        {
            TokenValidatorType.First => test.ValidateChar(subject.GetFirstChar()),
            TokenValidatorType.Middle => subject.GetMiddleChars() switch
            {
                null => false,
                string v => v.All(y => test.ValidateChar(y))
            },
            TokenValidatorType.Last => test.ValidateChar(subject.GetLastChar()),
            TokenValidatorType.All => subject.All(y => test.ValidateChar(y)),

            _ => throw new InvalidOperationException(),
        };
    }
}

public interface ITokenValidatorTest
{
    TokenValidatorType ValidatorType { get; }
    bool ValidateChar(char? chr);
}

public record TokenValidatorTest : ITokenValidatorTest
{
    public TokenValidatorTest(TokenValidatorType validatorType, Func<char, bool> match)
    {
        ValidatorType = validatorType;
        Match = match.NotNull();
    }

    public TokenValidatorType ValidatorType { get; }
    public Func<char, bool> Match { get; } = null!;

    public bool ValidateChar(char? chr) => chr switch { null => false, char c => Match(c) };
}

public record TokenValidatorLetterNumber : ITokenValidatorTest
{
    public TokenValidatorLetterNumber(TokenValidatorType validatorType) => ValidatorType = validatorType;
    public TokenValidatorType ValidatorType { get; }
    public bool ValidateChar(char? chr) => chr switch { null => false, char c => char.IsLetterOrDigit(c) };
}
