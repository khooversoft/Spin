using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace MessageNet.sdk.Protocol
{
    public record Header
    {
        public string Name { get; init; } = null!;

        public string? Value { get; init; }
    }


    public static class HeaderExtensions
    {
        public static void Verify(this Header subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Name.VerifyNotEmpty(nameof(subject.Name));
        }
    }
}
