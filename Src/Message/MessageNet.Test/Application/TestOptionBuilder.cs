using MessageNet.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Application;

namespace MessageNet.Test.Application
{
    internal class TestOptionBuilder : OptionBuilder
    {
        protected override Stream GetResourceStream(RunEnvironment runEnvironment) => runEnvironment.GetResourceStream(typeof(TestOptionBuilder), "MessageNet.Test.Application");
    }
}
