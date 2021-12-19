using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace Toolbox.Tools
{
    public static class Compiler
    {
        public static string Location([CallerMemberName] string function = "", [CallerFilePath] string path = "", [CallerLineNumber] int lineNumber = 0) =>
            "[Function=" + function + ", path=" + path + ", lineNumber=" + lineNumber.ToString() + "]";
    }
}
