using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;

namespace Toolbox.Tools
{
    public class StringVector : IEnumerable<string>
    {
        private string? _path;
        private readonly List<string> _values = new List<string>();

        public StringVector()
        {
        }

        public StringVector(string delimiter)
        {
            Delimiter = delimiter;
        }

        public string Delimiter { get; } = "/";

        public string Path { get => _path ??= string.Join(Delimiter, Values); }

        public IList<string> Values { get => _values; }

        public override string ToString() => Path;

        public StringVector Add(params string?[] values) => AddRange(values);

        public StringVector AddRange(IEnumerable<string?> values)
        {
            var valuesToAdd = (values ?? Array.Empty<string>())
                .SelectMany(x => Split(x))
                .Where(x => !x.IsEmpty());

            _values.AddRange(valuesToAdd);

            _path = null;
            return this;
        }


        // Conversions
        public static implicit operator string(StringVector vector) => vector.ToString();

        public static explicit operator StringVector(string path) => new StringVector() + path;


        // Additions

        public static StringVector operator +(StringVector subject, StringVector vector) => subject.Action(x => x.AddRange(vector.Values));

        public static StringVector operator +(StringVector subject, string? vector) => subject.Action(x => x.Add(vector));

        public static StringVector operator +(StringVector subject, string?[] vectors) => subject.Action(x => x.AddRange(vectors));


        // Equals support
        public override bool Equals(object? obj) => obj is StringVector vector && Enumerable.SequenceEqual(Values, vector.Values);

        public override int GetHashCode() => HashCode.Combine(Path);

        public static bool operator ==(StringVector? left, StringVector? right) => EqualityComparer<StringVector>.Default.Equals(left, right);

        public static bool operator !=(StringVector? left, StringVector? right) => !(left == right);


        // Support
        private string[] Split(string? subject) => (subject.ToNullIfEmpty() ?? string.Empty).Split(Delimiter);

        public IEnumerator<string> GetEnumerator()
        {
            foreach(var item in Values)
            {
                yield return item;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
