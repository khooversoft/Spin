using ArtifactStore.sdk.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Toolbox.Application;
using Toolbox.Extensions;
using Toolbox.Services;
using Toolbox.Tools;

namespace ArtifactCmd.Application
{
    internal static class OptionExtensions
    {
        private static IList<Action<Option>> verifications = new List<Action<Option>>
        {
            x => x.ApiKey.VerifyNotEmpty($"{nameof(x.ApiKey)} is required"),

            x => x.ArtifactUrl.VerifyNotEmpty($"{nameof(x.ArtifactUrl)} is required"),

            x => new [] { x.List, x.Get, x.Delete, x.Set }.Count(x => x == true).VerifyAssert(x => x == 1, "Only one command can be specified"),

            x => {
                if( !x.Get) return;

                x.File.VerifyNotEmpty($"{nameof(x.File)} is required for Get");
                x.Id.VerifyNotEmpty($"{nameof(x.Id)} is required for Get");
                ArtifactId.VerifyId(x.Id);
            },

            x => {
                if( !x.Set) return;

                x.File
                    .VerifyNotEmpty($"{nameof(x.File)} is required for Set")
                    .VerifyAssert(x => File.Exists(x), x => $"{x} does not exist for Set");

                x.Id.VerifyNotEmpty($"{nameof(x.Id)} is required for Set");
                ArtifactId.VerifyId(x.Id);
            },

            x =>
            {
                if( x.Delete ) x.Id.VerifyNotEmpty($"{nameof(x.Id)} is required for delete");
            },

            x =>
            {
                if( x.List) x.Namespace.VerifyNotEmpty($"{nameof(x.Namespace)} is required for list");
            }
        };

        public static void Verify(this Option option)
        {
            option.VerifyNotNull(nameof(option));

            verifications
                .ForEach(x => x(option));
        }

        internal static IReadOnlyList<string> GetHelp()
        {
            return new[]
            {
                "Artifact - Command Line Interface",
                "",
                "Help                               : Display help",
                "",
                "List artifacts",
                "  List                             : List command",
                "",
                "Get artifact (download).",
                "  Get                              : Get command",
                "  Id={id}                          : Artifact's ID",
                "  File={file}                      : Target file for download",
                "",
                "Delete artifact (download).",
                "  Delete                           : Get command",
                "  Id={id}                          : Artifact's ID",
                "",
                "Set artifact (download).",
                "  Set                              : Set command",
                "  Id={id}                          : Artifact's ID",
                "  File={file}                      : Artifact's file to upload",
                "",
                "",
                "Configuration For Artifact Server",
                "  SecretId={secretId}              : Use .NET Core configuration secret json file.  SecretId indicates which secret file to use.",
                "",
                "  ApiKey={key}                     : Artifact server's API Key (required)",
                "  ArtifactUrl={url}                : Artifact server's URL (required)",
            };
        }

        internal static void LogConfigurations(this Option option, ILogger logger)
        {
            ISecretFilter filter = new SecretFilter(option.ApiKey.ToEnumerable());

            logger.LogConfigurations(option, filter);
        }
    }
}
