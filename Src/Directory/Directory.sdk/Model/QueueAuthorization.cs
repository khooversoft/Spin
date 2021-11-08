using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Directory.sdk.Model
{
    public static class QueueAuthorization
    {
        public static (string KeyName, string AccessKey) Parse(string data)
        {
            data.VerifyNotEmpty(nameof(data));

            IReadOnlyDictionary<string, string> datDict = data
                .ToDictionary()!;

            datDict.TryGetValue("SharedAccessKeyName", out string? sharedAccessKeyName)
                .VerifyAssert(x => x == true, "SharedAccessKeyName not found in configuration");

            datDict.TryGetValue("SharedAccessKey", out string? sharedAccessKey)
                .VerifyAssert(x => x == true, "SharedAccessKey not found in configuration");

            return (sharedAccessKey!, sharedAccessKey!);
        }
    }
}
