using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tokenizer.Token;
using Toolbox.Types;
using Toolbox.Types.Structure;

namespace Toolbox.Parser.Grammar;

public class RepeatRule : TreeNode<IRule>, IRuleFunction
{
    public override string ToString() => $"[{this.GetType().Name}]";

    public IRuleValue? Match(Cursor<IToken> tokens)
    {
        return null;
    }
}
