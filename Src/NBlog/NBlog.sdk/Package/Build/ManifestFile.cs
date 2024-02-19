using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public record ManifestFile
{
    public ManifestFile(string file, IReadOnlyList<CommandNode> commands, ArticleManifest manifest)
    {
        File = file.NotEmpty();
        Commands = commands.NotNull();
        Manifest = manifest.Verify();
    }

    public string File { get; } = null!;
    public IReadOnlyList<CommandNode> Commands { get; } = Array.Empty<CommandNode>();
    public ArticleManifest Manifest { get; } = null!;
}


public static class ManifestFileTool
{
    public static async Task<Option<ManifestFile>> Read(string file, string basePath, ScopeContext context)
    {
        var articleManifestOption = await ReadManifest(file, basePath, context);
        if (articleManifestOption.IsError()) return articleManifestOption.ToOptionStatus<ManifestFile>();
        ArticleManifest articleManifest = articleManifestOption.Return();

        var commandOptions = ProcessCommands(file, articleManifest, basePath, context);
        if (commandOptions.IsError()) return commandOptions.ToOptionStatus<ManifestFile>();
        var commands = commandOptions.Return();

        return new ManifestFile(file, commands, articleManifest);
    }

    private static async Task<Option<ArticleManifest>> ReadManifest(string file, string basePath, ScopeContext context)
    {
        file.NotEmpty();
        basePath.NotEmpty();

        string fileId = file[(basePath.Length + 1)..].Replace(@"\", "/");
        string pathFileId = Path.GetDirectoryName(fileId).NotNull().Replace(@"\", "/");
        context.LogInformation("Reading manifest file, file={file} fileId={fileId}, pathFileId={pathFileId}", file, fileId, pathFileId);

        string data = await File.ReadAllTextAsync(file);

        var model = data.ToObject<ArticleManifest>();
        if (model == null)
        {
            context.Location().LogError("Cannot deserialize json={file}", file);
            return (StatusCode.BadRequest, $"Cannot deserialize json={file}");
        }

        model = model with
        {
            ArticleId = resolveVariables(model.ArticleId),
            Commands = model.Commands.Select(x => resolveVariables(x)).ToArray(),
            Index = model.Index ?? model.GetIndexOrStartDate(),
        };


        if (!model.Validate(out var v))
        {
            v.LogStatus(context, "File={file} is not a valid manifest file", file);
            return v.ToOptionStatus<ArticleManifest>();
        }

        return model;


        string resolveVariables(string value) => value.NotEmpty()
            .Replace("{pathAndFile}", fileId)
            .Replace("{path}", pathFileId);
    }

    private static Option<IReadOnlyList<CommandNode>> ProcessCommands(string file, ArticleManifest articleManifest, string basePath, ScopeContext context)
    {
        string folder = Path.GetFullPath(file)
            .Func(Path.GetDirectoryName)
            .NotNull($"File={file} does not have directory name");

        var commands = articleManifest.GetCommands()
            .Select(x => x.IsFileReference switch
            {
                true => x with { FileIdValue = Path.Combine(folder, x.FileIdValue) },
                false => x,
            })
            .ToArray();

        if (commands.Length == 0)
        {
            context.LogError("Manifest={file} does not have any commands specified");
            return StatusCode.BadRequest;
        }

        var fileReferenceOption = VerifyFileReference(commands, file, context);
        if (fileReferenceOption.IsError()) return fileReferenceOption.ToOptionStatus<IReadOnlyList<CommandNode>>();

        var indexReferenceOption = VerifyIndexReference(commands, file, basePath, context);
        if (indexReferenceOption.IsError()) return indexReferenceOption.ToOptionStatus<IReadOnlyList<CommandNode>>();

        return commands;
    }

    private static Option VerifyFileReference(CommandNode[] commands, string file, ScopeContext context)
    {
        var findResult = commands
            .Where(x => x.IsFileReference)
            .Select(x => VerifyFile(x.FileIdValue, file))
            .OfType<string>()
            .ToArray();

        if (findResult.Length != 0)
        {
            string msg = findResult.Aggregate("Errors in local files" + Environment.NewLine, (a, x) => a += x + Environment.NewLine);
            context.LogError(msg);
            return StatusCode.BadRequest;
        }

        return StatusCode.OK;
    }

    private static Option VerifyIndexReference(CommandNode[] commands, string file, string basePath, ScopeContext context)
    {
        var findResult = commands
            .Where(x => x.IsIndexReference)
            .Select(check)
            .OfType<string>()
            .ToArray();

        if (findResult.Length != 0)
        {
            string msg = findResult.Aggregate("Errors in index files references" + Environment.NewLine, (a, x) => a += x + Environment.NewLine);
            context.LogError(msg);
            return StatusCode.BadRequest;
        }

        return StatusCode.OK;

        string? check(CommandNode commandNode)
        {
            string fullPath = Path.Combine(basePath, commandNode.FileId);
            return VerifyFile(fullPath, file);
        }
    }

    private static string? VerifyFile(string localFile, string file)
    {
        string? error = File.Exists(localFile) switch
        {
            false => $"File={localFile} does not exist, local file for manifest={file}",
            true => new FileInfo(localFile) switch
            {
                { Length: 0 } => $"File={localFile} is empty, local file for manifest={file}",
                _ => null,
            }
        };

        return error;
    }
}
