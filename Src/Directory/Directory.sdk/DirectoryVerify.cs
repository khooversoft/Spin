using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Directory.sdk
{
    public static class DirectoryVerify
    {
        public static void VerifyDirectoryId(this string id) => id.IsDirectoryIdValid()
            .VerifyAssert(x => x.Valid, x => x.Message);

        public static (bool Valid, string Message) IsDirectoryIdValid(this string id)
        {
            if (id.IsEmpty()) return (false, "Id required");

            return (
                id.All(y => char.IsLetterOrDigit(y) || y == '.' || y == '-' || y == '@' || y == '/'),
                $"{id} is not valid, Valid Id must be letter, number, '/', '.', '@', or '-'"
                );
        }
    }
}
