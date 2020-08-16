using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox
{
    public static class EnumerableExtensions
    {
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
    }
}
