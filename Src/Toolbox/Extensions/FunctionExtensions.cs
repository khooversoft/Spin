using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox
{
    public static class FunctionExtensions
    {
        /// <summary>
        /// Execute function
        /// </summary>
        /// <typeparam name="T">subject type</typeparam>
        /// <typeparam name="TResult">return type</typeparam>
        /// <param name="subject">subject</param>
        /// <param name="function">lambda execute</param>
        /// <returns>return from lambda</returns>
        public static TResult Func<T, TResult>(this T subject, Func<T, TResult> function) => function.VerifyNotNull(nameof(subject))(subject);

        /// <summary>
        /// Execute action
        /// </summary>
        /// <typeparam name="T">any type</typeparam>
        /// <param name="subject">subject</param>
        /// <param name="action">action</param>
        /// <returns>subject</returns>
        public static T Action<T>(this T subject, Action<T> action)
        {
            subject.VerifyNotNull(nameof(subject));
            action.VerifyNotNull(nameof(action));

            action(subject);
            return subject;
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
