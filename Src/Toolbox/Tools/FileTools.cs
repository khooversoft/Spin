using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Services;

namespace Toolbox.Tools
{
    public static class FileTools
    {
        public static string WriteResourceToTempFile(string fileName, string folder, Type type, string resourceId)
        {
            @fileName.VerifyNotEmpty(nameof(fileName));
            folder.VerifyNotEmpty(nameof(folder));

            string filePath = Path.Combine(Path.GetTempPath(), folder, fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using var stream = GetResourceStream(type, resourceId);
            WriteStreamToFile(stream, filePath);

            return filePath;
        }

        /// <summary>
        /// Get stream from assembly's resources
        /// </summary>
        /// <param name="type">type int the assembly that has the resource</param>
        /// <param name="streamId">resource id</param>
        /// <returns>stream</returns>
        public static Stream GetResourceStream(this Type type, string streamId) =>
                Assembly.GetAssembly(type.VerifyNotNull(nameof(type)))!
                    .GetManifestResourceStream(streamId.VerifyNotEmpty(nameof(streamId)))
                    .VerifyNotNull($"Cannot find {streamId} in assembly's resource");

        /// <summary>
        /// Read resource and convert to string
        /// </summary>
        /// <param name="type">type int the assembly that has the resource</param>
        /// <param name="streamId">resource id</param>
        /// <returns>resource as string</returns>
        public static string GetResourceAsString(this Type type, string streamId)
        {
            using Stream stream = type.GetResourceStream(streamId);
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        public static void WriteStreamToFile(this Stream stream, string file)
        {
            using Stream writeFile = new FileStream(file, FileMode.Create);
            stream.CopyTo(writeFile);
        }
    }
}
