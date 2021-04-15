using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Identity.sdk.Types
{
    public record UserId
    {
        public UserId(string id)
        {
            Id = id;
            Verify(Id);
        }

        public string Id { get; }

        public static explicit operator UserId(string id) => new UserId(id);

        public static explicit operator string(UserId userId) => userId.ToString();

        public static UserId FromBase64(string base64) => new UserId(Encoding.UTF8.GetString(Convert.FromBase64String(base64)));

        public string ToBase64() => Convert.ToBase64String(Encoding.UTF8.GetBytes(Id));

        public override string ToString() => Id;

        public static void Verify(string id)
        {
            id.VerifyNotEmpty(nameof(id));
            id.VerifyAssert(x => x.All(y => char.IsLetterOrDigit(y) || y == '.' || y == '-' || y == '@'), "Valid Id must be letter, number, '.', '@', or '-'");

            id.VerifyAssert(x => char.IsLetter(x[0]), x => $"{x} Must start with letter");
            id.VerifyAssert(x => char.IsLetterOrDigit(x[^1]), x => "{x} must end with letter or number");
            id.IndexOf('@').VerifyAssert(x => x > 0 && x < id.Length, x => "{x} must have '@'");
        }
    }
}
