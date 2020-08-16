using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Tools;

namespace Toolbox.Services
{
    public class PropertyResolver : IPropertyResolver
    {
        private readonly IDictionary<string, string> _property;

        public PropertyResolver(IEnumerable<KeyValuePair<string, string>> properties)
        {
            properties.VerifyNotNull(nameof(properties));

            _property = new Dictionary<string, string>(properties, StringComparer.OrdinalIgnoreCase);
        }

        public string Resolve(string subject)
        {
            if (string.IsNullOrEmpty(subject)) return subject;

            return _property
                .Aggregate(subject, (acc, x) => acc.Replace($"{{{x.Key}}}", x.Value));
        }
    }
}
