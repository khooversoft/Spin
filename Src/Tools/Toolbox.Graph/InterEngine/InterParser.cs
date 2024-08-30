//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Extensions;
//using Toolbox.LangTools;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph.InterEngine;

//public static class InterParser
//{
//    public static Option<InterExecutionContext> Parse(IReadOnlyList<SyntaxPair> syntaxPairs)
//    {
//        syntaxPairs.NotNull();
//        var instructions = new Sequence<IGraphInstruction>();

//        var pContext = new InterContext(syntaxPairs);



//        while (pContext.Cursor.TryPeekValue(out var _))
//        {
//            if( 
//        }



//        InterExecutionContext context = new InterExecutionContext(instructions);
//        return context;
//    }
//}
