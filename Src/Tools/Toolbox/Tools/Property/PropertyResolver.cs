using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Toolbox.Extensions;

namespace Toolbox.Tools.Property
{
    public class PropertyResolver : IPropertyResolver
    {
        private readonly IDictionary<string, string> _property;

        public PropertyResolver(IEnumerable<KeyValuePair<string, string>> properties)
        {
            properties.VerifyNotNull(nameof(properties));

            _property = new Dictionary<string, string>(properties, StringComparer.OrdinalIgnoreCase);
        }

        [return: NotNullIfNotNull("subject")]
        public string? Resolve(string? subject)
        {
            if (subject.IsEmpty()) return subject;

            return _property
                .Aggregate(subject, (acc, x) => acc.Replace($"{{{x.Key}}}", x.Value, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasProperty(string subject)
        {
            if (subject.IsEmpty()) return false;
            return _property.Any(x => subject.IndexOf(x.Key, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}