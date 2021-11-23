using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTools.Application
{
    internal class AppOption
    {
        public IList<string> IgnoreTypes { get; set; } = new List<string>();
    }
}
