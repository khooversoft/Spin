using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class FileTool
{
    public readonly record struct FileHash
    {
        public string File { get; init; }
        public string Hash { get; init; }
    }

    public static async Task<Option<IReadOnlyList<FileHash>>> GetFileHashes(IEnumerable<string> files, ILogger logger)
    {
        logger.LogInformation("Calculating hash for all files");

        var fileList = files.ToArray();

        var fileHashes = await ActionParallel.RunAsync(fileList, getHash);
        var errors = fileHashes.Where(x => x.IsError()).Select(x => x.Error).ToArray();
        if (errors.Length > 0)
        {
            return (StatusCode.Conflict, errors.Join(';'));
        }

        return fileHashes.Select(x => x.Return()).OrderBy(x => x.File).ToArray();

        async Task<Option<FileHash>> getHash(string file)
        {
            var result = await GetFileHash(file, logger);
            if (result.IsError())
            {
                logger.LogError("Failed to get hash of file, file={file}", file);
                return (StatusCode.Conflict, $"Failed to get hash of file, file={file}");
            }

            return result;
        }
    }

    public static async Task<Option<FileHash>> GetFileHash(string file, ILogger logger)
    {
        if (!File.Exists(file)) return StatusCode.NotFound;

        using FileStream fileStream = File.OpenRead(file);
        using SHA256 hasher = SHA256.Create();

        try
        {
            fileStream.Position = 0;

            // Compute the hash of the fileStream.
            byte[] hashValue = await hasher.ComputeHashAsync(fileStream);

            return new FileHash
            {
                File = file,
                Hash = hashValue.ToHex(),
            }.ToOption();
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Failed to hash file={file}", file);
            return StatusCode.InternalServerError;
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError(ex, "Access Exception for hash file={file}", file);
            return StatusCode.InternalServerError;
        }
    }

    public static Option<string> ReadFile(string file, ILogger logger)
    {
        file.NotEmpty();

        if (!File.Exists(file))
        {
            logger.LogError("Cannot find file={file}", file);
            return StatusCode.NotFound;
        }

        string configJson = File.ReadAllText(file);
        if (configJson.IsEmpty())
        {
            logger.LogError("File={file} is empty", file);
            return StatusCode.NotFound;
        }

        return configJson;
    }

    public static Option<T> ReadFileAndDeserialize<T>(string file, IValidator<T> validator, ILogger logger)
    {
        validator.NotNull();

        var readOption = ReadFile(file, logger);
        if (readOption.IsError()) return readOption.ToOptionStatus<T>();

        var instance = readOption.Return().ToObject<T>();
        if (instance == null)
        {
            logger.LogError("Failed to deserialize configuration file={file}", file);
            return StatusCode.BadRequest;
        }

        var validation = validator.Validate(instance);
        if (validation.IsError())
        {
            logger.LogError("Configuration file={file} is invalid, error={error}", file, validation.ToString());
            return StatusCode.BadRequest;
        }

        return instance;
    }
}
