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
            properties.NotNull();

            _property = new Dictionary<string, string>(properties, StringComparer.OrdinalIgnoreCase);
        }

        public static IPropertyResolver CreateEmpty() => new PropertyResolver(Array.Empty<KeyValuePair<string, string>>());


        [return: NotNullIfNotNull("subject")]
        public string? Resolve(string? subject)
        {
            if (subject.IsEmpty()) return subject;

            var list = new List<KeyValuePair<string, string>>(_property);
            var index = new Cursor<KeyValuePair<string, string>>(list);

            while (HasProperty(subject))
            {
                if (!index.TryNextValue(out KeyValuePair<string, string> result))
                {
                    index.Reset();
                    continue;
                }

                subject = subject.Replace($"{{{result.Key}}}", result.Value, StringComparison.OrdinalIgnoreCase);
            }

            return subject;
        }

        public bool HasProperty(string subject)
        {
            if (subject.IsEmpty()) return false;
            return _property.Any(x => subject.IndexOf($"{{{x.Key}}}", StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}