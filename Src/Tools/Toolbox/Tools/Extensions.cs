using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace Toolbox.Tools
{
    public static class Extensions
    {
        public static UnixDate ToUnixDate(this long value) => new UnixDate(value);
    }
}
