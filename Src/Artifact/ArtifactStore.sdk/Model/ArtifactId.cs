using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace ArtifactStore.sdk.Model
{
    /// <summary>
    /// Id is a path of {namespace}/{resource}
    /// </summary>
    public record ArtifactId
    {
        public ArtifactId(string id)
        {
            id.VerifyNotEmpty(id);

            Id = id.ToLower();
            VerifyId(Id);
        }

        public string Id { get; }

        public string Namespace => Id.Split('/')[0];

        public string Path => Id.Split('/').Skip(1).Func(x => string.Join('/', x));

        public IReadOnlyList<string> PathItems => Id.Split('/').Skip(1).ToArray();

        public static explicit operator ArtifactId(string id) => new ArtifactId(id);

        public static explicit operator string(ArtifactId articleId) => articleId.ToString();

        public static ArtifactId FromBase64(string base64) => new ArtifactId(Encoding.UTF8.GetString(Convert.FromBase64String(base64)));

        public string ToBase64() => Convert.ToBase64String(Encoding.UTF8.GetBytes(Id));

        public override string ToString() => Id;

        public static void VerifyId(string id)
        {
            id.VerifyNotEmpty(nameof(id));

            id.VerifyAssert(x => x.All(y => char.IsLetterOrDigit(y) || y == '.' || y == '/' || y == '-' || y == '_'), "Valid Id must be letter, number, '.', '/', or '-'");

            id.Split('/')
                .VerifyAssert(x => x.Length > 1, "Missing namespace or id (ex: namespace/subject)")
                .VerifyAssert(x => x.All(y => !y.IsEmpty()), "path part is empty")
                .ForEach(x =>
                {
                    x.VerifyAssert(x => char.IsLetter(x[0]), $"{x} Must start with letter");
                    x.VerifyAssert(x => char.IsLetterOrDigit(x[^1]), "Must end with letter or number");
                });
        }
    }
}