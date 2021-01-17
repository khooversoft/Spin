using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Extensions
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
    }
}
