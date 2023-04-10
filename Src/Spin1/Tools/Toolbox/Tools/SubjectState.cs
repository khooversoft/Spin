using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Toolbox.Tools
{
    public class SubjectState<T> where T : class
    {
        private T? _subject;

        public SubjectState(T subject) => _subject = subject.NotNull();

        public T Subject => _subject.NotNull();

        public T? SubjectOrDefault => _subject;

        public T? GetAndClear() => Interlocked.Exchange(ref _subject, null);
    }

    public static class SubjectScopeExtensions
    {
        public static SubjectState<T> ToSubjectScope<T>(this T subject) where T : class => new SubjectState<T>(subject.NotNull());
    }
}
