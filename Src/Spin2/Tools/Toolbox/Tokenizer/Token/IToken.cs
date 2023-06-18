using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Tokenizer.Token
{
    /// <summary>
    ///  Standard token interface
    /// </summary>
    public interface IToken
    {
        string Value { get; }
    }
}