using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Abstractions.Extensions;
using Toolbox.Abstractions.Tools;

namespace Directory.sdk.Tools
{
    public static class QueueAuthorization
    {
        public static (string KeyName, string AccessKey) Parse(string data)
        {
            data.NotEmpty();

            IReadOnlyDictionary<string, string> datDict = data
                .ToDictionary()!;

            datDict.TryGetValue("SharedAccessKeyName", out string? sharedAccessKeyName)
                .Assert(x => x == true, "SharedAccessKeyName not found in configuration");

            datDict.TryGetValue("SharedAccessKey", out string? sharedAccessKey)
                .Assert(x => x == true, "SharedAccessKey not found in configuration");

            return (sharedAccessKeyName!, sharedAccessKey!);
        }
    }
}
