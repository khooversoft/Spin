using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Parser.Syntax;
using Toolbox.Tokenizer.Token;
using Toolbox.Types;
using Toolbox.Types.Structure;

namespace Toolbox.Parser.Grammar;


public interface IRule : ITreeNode
{
}


public interface IRuleValue : ITreeNode
{
    public static IRuleValue operator +(IRuleValue left, IRuleValue right)
    {
        left.Add(right);
        return left;
    }

    public static IRuleValue operator +(IRuleValue left, IEnumerable<IRuleValue> right)
    {
        left.AddRange(right);
        return left;
    }
}


public interface IRuleSingle : IRule
{
    IRuleValue? Match(IToken token);
}


public interface IRuleFunction
{
    IRuleValue? Match(Cursor<IToken> tokens);
}

