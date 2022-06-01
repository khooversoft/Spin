using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Convert a scalar value to enumerable
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="self">object to convert</param>
        /// <returns>enumerator</returns>
        public static IEnumerable<T> ToEnumerable<T>(this T self)
        {
            yield return self;
        }

        /// <summary>
        /// Execute 'action' on each item
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="subjects">types to process</param>
        /// <param name="action">action to execute</param>
        public static void ForEach<T>(this IEnumerable<T> subjects, Action<T> action)
        {
            subjects.NotNull();
            action.NotNull();

            foreach (var item in subjects)
            {
                action(item);
            }
        }

        /// <summary>
        /// Execute 'action' on each item
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="subjects">list to operate on</param>
        /// <param name="action">action to execute</param>
        public static void ForEach<T>(this IEnumerable<T> subjects, Action<T, int> action)
        {
            subjects.NotNull();
            action.NotNull();

            int index = 0;
            foreach (var item in subjects)
            {
                action(item, index++);
            }
        }

        /// <summary>
        /// Execute 'action' on each item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subjects"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task ForEachAsync<T>(this IEnumerable<T> subjects, Func<T, Task> action)
        {
            subjects.NotNull();
            action.NotNull();

            foreach (var item in subjects)
            {
                await action(item);
            }
        }

        /// <summary>
        /// Covert enumerable to stack, null will return empty stack
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subjects"></param>
        /// <returns>Stack<typeparamref name="T"/></returns>
        public static Stack<T> ToStack<T>(this IEnumerable<T>? subjects) => new Stack<T>(subjects ?? Array.Empty<T>());

        /// <summary>
        /// Shuffle list based on random crypto provider
        /// </summary>
        /// <typeparam name="T">type in list</typeparam>
        /// <param name="self">list to shuffle</param>
        /// <returns>shuffled list</returns>
        public static IReadOnlyList<T> Shuffle<T>(this IEnumerable<T> self)
        {
            self.NotNull();

            var list = self.ToList();

            var provider = RandomNumberGenerator.Create();
            int n = list.Count;

            while (n > 1)
            {
                var box = new byte[1];
                do
                {
                    provider.GetBytes(box);
                }
                while (!(box[0] < n * (Byte.MaxValue / n)));

                var k = (box[0] % n);
                n--;

                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

        public static IEnumerable<T> ToSafe<T>(this IEnumerable<T>? list) => (list ?? Array.Empty<T>());
    }
}
