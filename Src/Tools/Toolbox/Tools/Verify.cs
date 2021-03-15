using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
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
        public static void Assert(bool state, string message)
        {
            if (!state) throw new ArgumentException(message);
        }

        /// <summary>
        /// Assert test and throw exception with message
        /// </summary>
        /// <typeparam name="T">type of exception</typeparam>
        /// <param name="test">test</param>
        /// <param name="message">exception message optional</param>
        [DebuggerStepThrough]
        public static void Assert<T>(bool test, string message) where T : Exception
        {
            if (test) return;
            message = message ?? throw new ArgumentException(nameof(message));

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
        public static T VerifyAssert<T>(this T subject, Func<T, bool> test, string message)
        {
            if (test(subject)) return subject;

            message.VerifyNotEmpty(nameof(message));
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
        public static T VerifyAssert<T>(this T subject, Func<T, bool> test, Func<T, string> getMessage)
        {
            if (test(subject)) return subject;

            getMessage.VerifyNotNull(nameof(getMessage));
            throw new ArgumentException(getMessage(subject));
        }

        /// <summary>
        /// Assert test and throw exception with message
        /// </summary>
        /// <typeparam name="T">type of exception</typeparam>
        /// <param name="test">test</param>
        /// <param name="message">exception message optional</param>
        [DebuggerStepThrough]
        public static T VerifyAssert<T, TException>(this T subject, Func<T, bool> test, Func<T, string> getMessage) where TException : Exception
        {
            if (test(subject)) return subject;

            getMessage.VerifyNotNull(nameof(getMessage));
            throw (Exception)Activator.CreateInstance(typeof(TException), getMessage(subject))!;
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
        public static T VerifyNotNull<T>([NotNull] this T subject, string name)
        {
            if (subject == null || EqualityComparer<T>.Default.Equals(subject, default!)) throw new ArgumentNullException(name);
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
        public static string VerifyNotEmpty([NotNull] this string? subject, string name)
        {
            if (subject.IsEmpty()) throw new ArgumentNullException(name);
            return subject;
        }
    }
}
