using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tasks">task to join</param>
        /// <returns>task</returns>
        public static Task WhenAll(this IEnumerable<Task> tasks)
        {
            tasks.NotNull();

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
            tasks.NotNull();

            return Task.WhenAll(tasks);
        }
    }
}
