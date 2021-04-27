using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Identity.sdk.Models
{
    public record IdentityNamespaces
    {
        public string Signature { get; init; } = null!;

        public string Subscription { get; init; } = null!;

        public string Tenant { get; init; } = null!;

        public string User { get; init; } = null!;
    }

    public static class IdentityNamespacesExtensions
    {
        public static void Verify(this IdentityNamespaces subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Signature.VerifyNotEmpty(nameof(subject.Signature));
            subject.Subscription.VerifyNotEmpty(nameof(subject.Subscription));
            subject.Tenant.VerifyNotEmpty(nameof(subject.Tenant));
            subject.User.VerifyNotEmpty(nameof(subject.User));
        }
    }
}
