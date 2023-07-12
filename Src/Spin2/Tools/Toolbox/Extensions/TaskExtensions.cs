using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Extensions;

public static class TaskExtensions
{
    public static Task<T> ToTaskResult<T>(this T value) => Task.FromResult<T>(value);
}
