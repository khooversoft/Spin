using MessageNet.sdk.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;

namespace MessageNet.sdk.Models
{
    public record RegisterSync
    {
        public MessageUrl Url { get; init; } = null!;

        public string CallbackUri { get; init; } = null!;
    }


    public static class RegisterSyncExtensions
    {
        public static bool IsValid(this RegisterSync subject)
        {
            if (subject == null) return false;
            if (subject.CallbackUri.IsEmpty()) return false;

            return true;
        }
    }
}
