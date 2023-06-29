using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;

namespace Toolbox.Extensions;

public static class StreamExtensions
{
    public static async Task<string> ReadStringStream(this Stream stream)
    {
        using StreamReader sr = new StreamReader(stream);
        return await sr.ReadToEndAsync();
    }
}
