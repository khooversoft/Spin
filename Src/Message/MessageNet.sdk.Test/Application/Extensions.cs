using MessageNet.sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace MessageNet.sdk.Test.Application
{
    public static class Extensions
    {
        public static void Verify(this TestOption subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Nodes.VerifyNotNull($"{nameof(subject.Nodes)} must be specified");
            subject.Nodes.VerifyAssert(x => x.Count > 0, $"{nameof(subject.Nodes)} must be specified");
            subject.BusNamespace.Verify();
        }
    }
}
