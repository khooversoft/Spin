using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Application
{
    public enum RunEnvironment
    {
        Unknown,
        Local,
        Dev,
        PreProd,
        Prod
    }
}
