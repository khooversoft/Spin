using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Tools;
using Toolbox.Extensions;

namespace ToolBox.Azure.Test.Application
{
    public class TestOptionBuilder
    {
        private const string resourceId = "ToolBox.Azure.Test.Application.TestConfig.json";

        public TestOptionBuilder()
        {
        }

        public DataLakeStoreOption Build(params string[] args)
        {

            using Stream configStream = GetConfigStream();

            DataLakeStoreOption option = new ConfigurationBuilder()
                .AddJsonStream(configStream)
                .AddUserSecrets("Toolbox.Test")
                .AddCommandLine(args)
                .Build()
                .Bind<DataLakeStoreOption>();


            return option;
        }

        public static string WriteResourceToFile(string fileName)
        {
            string filePath = Path.Combine(Path.GetTempPath(), nameof(TestOptionBuilder), fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using Stream stream = GetConfigStream();
            using Stream writeFile = new FileStream(filePath, FileMode.Create);
            stream.CopyTo(writeFile);

            return filePath;
        }

        private static Stream GetConfigStream() =>
            Assembly.GetAssembly(typeof(TestOptionBuilder))!
            .GetManifestResourceStream("ToolBox.Azure.Test.Application.TestConfig.json")
            .VerifyNotNull($"Cannot find resource {resourceId}");
    }
}