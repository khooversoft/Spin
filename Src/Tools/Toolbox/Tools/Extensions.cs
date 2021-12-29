using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Model;
using Toolbox.Types;

namespace Toolbox.Tools
{
    public static class Extensions
    {
        public static UnixDate ToUnixDate(this long value) => new UnixDate(value);

        public static async Task<IReadOnlyList<T>> ToList<T>(this BatchSetCursor<T> batch, CancellationToken token)
        {
            batch.VerifyNotNull(nameof(batch));

            var list = new List<T>();
            while (true)
            {
                BatchSet<T>? result = await batch.ReadNext(token);
                if (result.IsEndSignaled) break;

                list.AddRange(result.Records);
            }

            return list;
        }

        public static StringVector ToStringVector(this IEnumerable<string> list, string? delimiter = null)
        {
            var vector = delimiter == null ? new StringVector() : new StringVector(delimiter);
            vector.AddRange(list);

            return vector;
        }

        public static StringVector ToStringVector<T>(this IEnumerable<T> list, string? delimiter = null)
        {
            var vector = delimiter == null ? new StringVector() : new StringVector(delimiter);
            vector.AddRange(list.Select(x => x?.ToString()));

            return vector;
        }
    }
}