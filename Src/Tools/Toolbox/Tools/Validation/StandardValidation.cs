using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Toolbox.Tools;

public static partial class StandardValidation
{

    public static bool IsName(string value) => NameRegex().IsMatch(value);
    public const string NameError = "Invalid character(s), start with alpha and only alpha, numberic or / - : @ . allowed";

    public static bool IsDescrption(string value) => DescriptionRegex().IsMatch(value);
    public const string DescriptionError = "Invalid character(s), only only alpha, numberic or \" / - : ; @ . allowed";



    [GeneratedRegex(@"^[a-zA-Z][a-zA-Z0-9\/\-\:\@\.]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex NameRegex();


    [GeneratedRegex(@"^[a-zA-Z0-9\s'\"",;*@\.\#]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex DescriptionRegex();
}
