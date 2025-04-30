//using System.Diagnostics;
//using System.Runtime.CompilerServices;

//namespace Toolbox.Tools.Should;

//[DebuggerStepThrough]
//public static class ShouldFunctionExtensions
//{
//    public static ShouldContext<Func<T>> Should<T>(
//        this Func<T> subject,
//        [CallerMemberName] string function = "",
//        [CallerFilePath] string path = "",
//        [CallerLineNumber] int lineNumber = 0,
//        [CallerArgumentExpression("subject")] string name = ""
//        )
//    {
//        return new ShouldContext<Func<T>>(subject, function, path, lineNumber, name);
//    }

//    public static ShouldContext<Func<T>> NotThrow<T>(this ShouldContext<Func<T>> subject, string? because = null)
//    {
//        try
//        {
//            subject.Value.Invoke();
//        }
//        catch (Exception ex)
//        {
//            subject.ThrowException($"Exception was thrown", because, ex);
//        }

//        return subject;
//    }

//    public static ShouldContext<Func<T>> Throw<T, TException>(this ShouldContext<Func<T>> subject, string? because = null)
//        where TException : Exception
//    {
//        try
//        {
//            subject.Value.Invoke();
//        }
//        catch (Exception ex)
//        {
//            if (ex.GetType() != typeof(TException)) subject.ThrowException($"Exception was not thrown", because);
//            return subject;
//        }

//        subject.ThrowException($"Exception was not thrown", because);
//        throw new UnreachableException();
//    }
//}
