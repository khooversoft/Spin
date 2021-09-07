using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Tools
{
    /// <summary>
    /// Utility class for creating <see cref="IEqualityComparer{T}"/> instances 
    /// from Lambda expressions.
    /// </summary>
    public class EqualityComparerFactory
    {
        /// <summary>Creates the specified <see cref="IEqualityComparer{T}" />.</summary>
        /// <typeparam name="T">The type to compare.</typeparam>
        /// <param name="equals">The equals delegate.</param>
        /// <returns>An instance of <see cref="IEqualityComparer{T}" />.</returns>
        public static IEqualityComparer<T> Create<T>(Func<T?, T?, bool> equals) => new Comparer<T>(equals);

        /// <summary>Creates the specified <see cref="IEqualityComparer{T}" />.</summary>
        /// <typeparam name="T">The type to compare.</typeparam>
        /// <param name="getHashCode">The get hash code delegate.</param>
        /// <param name="equals">The equals delegate.</param>
        /// <returns>An instance of <see cref="IEqualityComparer{T}" />.</returns>
        public static IEqualityComparer<T> Create<T>(Func<T?, T?, bool> equals, Func<T, int> getHashCode) => new Comparer<T>(equals, getHashCode);

        private class Comparer<T> : IEqualityComparer<T>
        {
            private readonly Func<T?, T?, bool> _equals;
            private readonly Func<T, int>? _getHashCode;

            public Comparer(Func<T?, T?, bool> equals)
            {
                _equals = equals;
            }

            public Comparer(Func<T?, T?, bool> equals, Func<T, int>? getHashCode)
            {
                _equals = equals;
                _getHashCode = getHashCode;
            }

            public bool Equals(T? x, T? y) => _equals(x, y);

            public int GetHashCode(T obj) => _getHashCode?.Invoke(obj) ?? obj?.GetHashCode() ?? 0;
        }
    }
}
