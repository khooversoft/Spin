using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Toolbox.Tools
{
    public class SubjectScope<T> where T : class
    {
        private T? _subject;

        public SubjectScope(T subject) => _subject = subject.VerifyNotNull(nameof(subject));

        public T Subject => _subject.VerifyNotNull(nameof(Subject));

        public T? SubjectOrDefault => _subject;

        public T? GetAndClear() => Interlocked.Exchange(ref _subject, null!);
    }

    public static class SubjectScopeExtensions
    {
        public static SubjectScope<T> ToSubjectScope<T>(this T subject) where T : class => new SubjectScope<T>(subject.VerifyNotNull(nameof(subject)));
    }
}
