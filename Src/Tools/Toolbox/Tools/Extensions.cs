using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Model;
using Toolbox.Types;

namespace Toolbox.Tools
{
    public static class Extensions
    {
        public static UnixDate ToUnixDate(this long value) => new UnixDate(value);

        public static async Task<IReadOnlyList<T>> ToList<T>(this BatchSetHttpCursor<T> batch, CancellationToken token)
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
    }
}