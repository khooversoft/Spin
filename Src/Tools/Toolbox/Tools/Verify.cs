using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;

namespace Toolbox.Tools
{
    public static class Verify
    {
        /// <summary>
        /// Assert test
        /// </summary>
        /// <param name="state">state to test</param>
        /// <param name="message">message</param>
        [DebuggerStepThrough]
        public static void Assert(
                bool state,
                string message,
                ILogger? logger = null,
                [CallerMemberName] string function = "",
                [CallerFilePath] string path = "",
                [CallerLineNumber] int lineNumber = 0
            )
        {
            if (!state)
            {
                message += ", " + FormatCaller(function, path, lineNumber);
                logger?.LogError(message);
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Assert test and throw exception with message
        /// </summary>
        /// <typeparam name="T">type of exception</typeparam>
        /// <param name="test">test</param>
        /// <param name="message">exception message optional</param>
        [DebuggerStepThrough]
        public static void Assert<T>(
                bool test,
                string message,
                ILogger? logger = null,
                [CallerMemberName] string function = "",
                [CallerFilePath] string path = "",
                [CallerLineNumber] int lineNumber = 0
            ) where T : Exception
        {
            if (test) return;
            message.VerifyNotEmpty(nameof(message));

            message += ", " + FormatCaller(function, path, lineNumber);
            logger?.LogError(message);
            throw (Exception)Activator.CreateInstance(typeof(T), message)!;
        }

        /// <summary>
        /// Verify state
        /// </summary>
        /// <typeparam name="T">any type</typeparam>
        /// <param name="subject">subject</param>
        /// <param name="test">test func</param>
        /// <param name="message">message</param>
        /// <returns>subject</returns>
        [DebuggerStepThrough]
        public static T VerifyAssert<T>(
                this T subject,
                Func<T, bool> test,
                string message,
                ILogger? logger = null,
                [CallerMemberName] string function = "",
                [CallerFilePath] string path = "",
                [CallerLineNumber] int lineNumber = 0
            )
        {
            if (test(subject)) return subject;

            message.VerifyNotEmpty(nameof(message));

            message += ", " + FormatCaller(function, path, lineNumber);
            logger?.LogError(message);
            throw new ArgumentException(message);
        }

        /// <summary>
        /// Verify state
        /// </summary>
        /// <typeparam name="T">any type</typeparam>
        /// <param name="subject">subject</param>
        /// <param name="test">test func</param>
        /// <param name="getMessage">get message</param>
        /// <returns>subject</returns>
        [DebuggerStepThrough]
        public static T VerifyAssert<T>(
                this T subject,
                Func<T, bool> test,
                Func<T, string> getMessage,
                ILogger? logger = null,
                [CallerMemberName] string function = "",
                [CallerFilePath] string path = "",
                [CallerLineNumber] int lineNumber = 0
            )
        {
            if (test(subject)) return subject;

            getMessage.VerifyNotNull(nameof(getMessage));
            string msg = getMessage(subject) + ", " + FormatCaller(function, path, lineNumber);
            logger?.LogError(msg);
            throw new ArgumentException(msg);
        }

        /// <summary>
        /// Assert test and throw exception with message
        /// </summary>
        /// <typeparam name="T">type of exception</typeparam>
        /// <param name="test">test</param>
        /// <param name="message">exception message optional</param>
        [DebuggerStepThrough]
        public static T VerifyAssert<T, TException>(
                this T subject,
                Func<T, bool> test,
                Func<T, string> getMessage,
                ILogger? logger = null,
                [CallerMemberName] string function = "",
                [CallerFilePath] string path = "",
                [CallerLineNumber] int lineNumber = 0
            ) where TException : Exception
        {
            if (test(subject)) return subject;

            getMessage.VerifyNotNull(nameof(getMessage));
            string msg = getMessage(subject) + ", " + FormatCaller(function, path, lineNumber);
            logger?.LogError(msg);
            throw (Exception)Activator.CreateInstance(typeof(TException), msg)!;
        }

        /// <summary>
        /// Verify subject is not null or default
        /// </summary>
        /// <typeparam name="T">subject type</typeparam>
        /// <param name="subject">subject</param>
        /// <param name="name">name of subject or message</param>
        /// <returns>subject</returns>
        [DebuggerStepThrough]
        [return: NotNull]
        public static T VerifyNotNull<T>(
                [NotNull] this T subject,
                string name,
                ILogger? logger = null,
                [CallerMemberName] string function = "",
                [CallerFilePath] string path = "",
                [CallerLineNumber] int lineNumber = 0
            )
        {
            if (subject == null || EqualityComparer<T>.Default.Equals(subject, default!))
            {
                string msg = $"Null object: {name}, {FormatCaller(function, path, lineNumber)}";
                logger?.LogError(msg);
                throw new ArgumentNullException(msg);
            }

            return subject;
        }

        /// <summary>
        /// Verify subject is not null or empty
        /// </summary>
        /// <param name="subject">subject</param>
        /// <param name="name">name of subject or message</param>
        /// <returns>subject</returns>
        [DebuggerStepThrough]
        [return: NotNull]
        public static string VerifyNotEmpty(
                [NotNull] this string? subject,
                string name,
                ILogger? logger = null,
                [CallerMemberName] string function = "",
                [CallerFilePath] string path = "",
                [CallerLineNumber] int lineNumber = 0
            )
        {
            if (subject.IsEmpty())
            {
                string msg = $"Empty or null string: {name}, {FormatCaller(function, path, lineNumber)}";
                logger?.LogError(msg);
                throw new ArgumentNullException(msg);
            }

            return subject;
        }

        private static string FormatCaller(string function, string path, int lineNumber) => $"Function={function}, File={path}, LineNumber={lineNumber}";
    }
}
