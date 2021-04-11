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
            Verify();
        }

        public string Id { get; }

        public static explicit operator IdentityId(string id) => new IdentityId(id);

        public static explicit operator string(IdentityId articleId) => articleId.ToString();

        public static IdentityId FromBase64(string base64) => new IdentityId(Encoding.UTF8.GetString(Convert.FromBase64String(base64)));

        public string ToBase64() => Convert.ToBase64String(Encoding.UTF8.GetBytes(Id));

        public override string ToString() => Id;

        private void Verify()
        {
            Id.VerifyNotEmpty(nameof(Id));
            Id.VerifyAssert(x => x.All(y => char.IsLetterOrDigit(y) || y == '.' || y == '-'), "Valid Id must be letter, number, '.', or '-'");

            Id.VerifyAssert(x => char.IsLetter(x[0]), x => $"{x} Must start with letter");
            Id.VerifyAssert(x => char.IsLetterOrDigit(x[^1]), x => "{x} must end with letter or number");
        }
    }
}
