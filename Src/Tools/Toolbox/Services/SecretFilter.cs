using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Services
{
    public class SecretFilter : ISecretFilter
    {
        private readonly HashSet<string> _secrets;

        public SecretFilter() => _secrets = new HashSet<string>();

        public SecretFilter(IEnumerable<string> secrets)
        {
            secrets.NotNull(nameof(secrets));

            _secrets = new HashSet<string>(secrets);
        }

        public string? FilterSecrets(string? data, string replaceSecretWith = "***")
        {
            if (data.ToNullIfEmpty() == null || _secrets.Count == 0) return data;
            replaceSecretWith.NotEmpty(nameof(replaceSecretWith));

            return _secrets.Aggregate(data!, (acc, x) => acc.Replace(x, replaceSecretWith));
        }
    }
}
