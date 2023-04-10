using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Extensions
{
    public static class ArrayExtensions
    {
        public static byte[] RemoveBOM(this byte[] data)
        {
            data.NotNull();

            if (data.Length > 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
            {
                return data[3..];
            }

            return data;
        }
    }
}
