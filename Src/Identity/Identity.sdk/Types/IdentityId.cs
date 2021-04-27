using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Identity.sdk.Types
{
    public record IdentityId
    {
        public IdentityId(string id)
        {
            Id = id;
            Verify(Id);
        }

        public string Id { get; }

        public static explicit operator IdentityId(string id) => new IdentityId(id);

        public static explicit operator string(IdentityId identityId) => identityId.ToString();

        public override string ToString() => Id;

        public static void Verify(string id)
        {
            id.VerifyNotEmpty(nameof(id));
            id.VerifyAssert(x => x.All(y => char.IsLetterOrDigit(y) || y == '.' || y == '-' || y == '_' ), "Valid Id must be letter, number, '.', '-', '_'");

            id.VerifyAssert(x => char.IsLetter(x[0]), x => $"{x} Must start with letter");
            id.VerifyAssert(x => char.IsLetterOrDigit(x[^1]), x => "{x} must end with letter or number");
        }
    }
}
