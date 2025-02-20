using System.Text.RegularExpressions;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public static partial class StandardValidation
{

    public static bool IsName(string? value) => value.IsNotEmpty() switch
    {
        true => NameRegex().IsMatch(value),
        false => false,
    };

    public const string NameError = "Invalid character(s), start with alpha and only alpha, numberic or / - : @ . allowed";

    public static bool IsDescrption(string? value) => value.IsNotEmpty() switch
    {
        true => DescriptionRegex().IsMatch(value),
        false => false,
    };

    public const string DescriptionError = "Invalid character(s), only only alpha, numberic, or symbols allowed";

    public static bool IsEmail(string? value) => value.IsNotEmpty() switch
    {
        true => EmailRegex().IsMatch(value),
        false => false,
    };

    public const string EmailError = "Invalid email format";


    [GeneratedRegex(@"^[a-zA-Z][a-zA-Z0-9\/\-\:\@\.]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex NameRegex();


    [GeneratedRegex(@"[A-Za-z0-9!\-\/:-@[-`{-~ ]", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex DescriptionRegex();


    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();
}
