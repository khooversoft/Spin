using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.LangTools.Pattern;

public class PmRoot : PatternBase
{
    public PmRoot(string? name = null) : base(name) { }
    public override Option<Sequence<IPatternSyntax>> Process(PatternContext pContext)
    {
        return this.MatchSyntaxSegement(pContext);
    }

    public static PmRoot operator +(PmRoot subject, IPatternSyntax syntax) => subject.Action(x => x.Add(syntax));
    //public static PmRoot operator +(PmRoot subject, IPatternBase<IPatternSyntax> set) => subject.Action(x => x.AddRange(set.Children));
}
