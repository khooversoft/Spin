using MessageNet.sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Extensions;

namespace MessageNet.Application
{
    public static class Extensions
    {
        public static void Verify(this Option subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Nodes.VerifyNotNull($"{nameof(subject.Nodes)} must be specified");
            subject.Nodes.VerifyAssert(x => x.Count > 0, $"{nameof(subject.Nodes)} must have 1 or more");
            subject.Nodes.ForEach(x => x.Verify());
            subject.BusNamespace.Verify();
            subject.ApiKey.VerifyNotEmpty($"{nameof(subject.ApiKey)} required");
        }
    }
}
