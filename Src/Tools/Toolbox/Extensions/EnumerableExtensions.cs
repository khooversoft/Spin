using System;
using System.Collections.Generic;
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
        /// 
        /// </summary>
        /// <param name="tasks">task to join</param>
        /// <returns>task</returns>
        public static Task WhenAll(this IEnumerable<Task> tasks)
        {
            tasks.VerifyNotNull(nameof(tasks));

            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="tasks">task to join</param>
        /// <returns>array of types</returns>
        public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
        {
            tasks.VerifyNotNull(nameof(tasks));

            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Execute 'action' on each item
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="subjects">types to process</param>
        /// <param name="action">action to execute</param>
        public static void ForEach<T>(this IEnumerable<T> subjects, Action<T> action)
        {
            subjects.VerifyNotNull(nameof(subjects));
            action.VerifyNotNull(nameof(action));

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
            subjects.VerifyNotNull(nameof(subjects));
            action.VerifyNotNull(nameof(action));

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
            subjects.VerifyNotNull(nameof(subjects));
            action.VerifyNotNull(nameof(action));

            foreach (var item in subjects)
            {
                await action(item);
            }
        }
    }
}
