using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Toolbox.Tools
{
    /// <summary>
    /// Immutable string parts or vectors
    /// </summary>
    public class StringVector : IEnumerable<string>
    {
        private readonly string[] _parts;

        /// <summary>
        /// Default empty
        /// </summary>
        private StringVector()
            : this(Enumerable.Empty<string>(), "/")
        {
        }

        /// <summary>
        /// Default empty
        /// </summary>
        public StringVector(string value)
            : this(value, "/")
        {
        }

        public StringVector(string value, string delimiter)
        {
            value.VerifyNotEmpty(nameof(value));
            delimiter.VerifyNotEmpty(nameof(delimiter));

            Delimiter = delimiter;
            _parts = value.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries).ToArray();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parts">parts of path</param>
        /// <param name="delimiter">delimiter</param>
        /// <param name="hasRoot">has root</param>
        public StringVector(IEnumerable<string> parts, string delimiter)
        {
            delimiter.VerifyNotNull(nameof(parts));

            Delimiter = delimiter;

            _parts = parts
                .Where(x => !x.IsEmpty())
                .SelectMany(x => x.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries))
                .ToArray();
        }

        /// <summary>
        /// Empty path with root
        /// </summary>
        public static StringVector Empty { get; } = new StringVector();

        public string this[int index] => _parts[index];

        public int Count => _parts.Length;

        public string Delimiter { get; }

        /// <summary>
        /// Try get part at index
        /// </summary>
        /// <param name="index">index of part</param>
        /// <param name="value">returned value</param>
        /// <returns>true if part exist, false if not</returns>
        public bool TryGet(int index, [MaybeNullWhen(returnValue: false)] out string? value)
        {
            value = default!;
            if (index < 0 || index >= _parts.Length) return false;
            value = _parts[index];
            return true;
        }

        /// <summary>
        /// Convert parts to string
        /// </summary>
        /// <returns>path</returns>
        public override string ToString() => string.Join(Delimiter, _parts);

        /// <summary>
        /// Create new immutable string path with parts added
        /// </summary>
        /// <param name="parts"></param>
        /// <returns>new immutable string path object</returns>
        public StringVector With(params string[] parts) => new StringVector(_parts.Concat(parts), Delimiter);

        public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)_parts).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<string>)_parts).GetEnumerator();

        public static StringVector Parse(params string[] values) => new StringVector(values, "/");

        /// <summary>
        /// Implicit conversion to string
        /// </summary>
        /// <param name="source">source to convert</param>
        public static implicit operator string(StringVector source) => source.ToString();

        /// <summary>
        /// Add operator, concatenate two string vectors
        /// </summary>
        /// <param name="subject">subject</param>
        /// <param name="rvalue">value to concatenate</param>
        /// <returns>new result string vector</returns>
        public static StringVector operator +(StringVector subject, StringVector rvalue) => subject.With(rvalue);

        /// <summary>
        /// Add operator, concatenate a string to a vector
        /// </summary>
        /// <param name="subject">subject</param>
        /// <param name="rvalue">value to concatenate</param>
        /// <returns>new result string vector</returns>
        public static StringVector operator +(StringVector subject, string rvalue) => subject.With(rvalue);
    }
}