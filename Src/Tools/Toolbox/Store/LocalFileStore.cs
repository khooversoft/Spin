using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class LocalFileStore : IFileStore
{
    private readonly ILogger<LocalFileStore> _logger;
    private readonly LocalFileStoreOption _localFileStoreOption;

    public LocalFileStore(LocalFileStoreOption localFileStoreOption, ILogger<LocalFileStore> logger)
    {
        _localFileStoreOption = localFileStoreOption.NotNull();
        _localFileStoreOption.Validate().ThrowOnError();

        _logger = logger.NotNull();
    }

    public async Task<Option<string>> Add(string path, DataETag data, ScopeContext context)
    {
        if (!FileStoreTool.IsPathValid(path)) return StatusCode.BadRequest;
        context = context.With(_logger);
        string filePath = BuildFilePath(path);

        try
        {
            if (File.Exists(filePath)) return StatusCode.Conflict;

            using (FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
            {
                await fs.WriteAsync(data.Data.AsMemory());
            }

            context.LogTrace("Add Path={path} with eTag={eTag}, FilePath={filePath}", path, data.ETag, filePath);
        }
        catch (IOException ex)
        {
            return (StatusCode.Conflict, ex.Message);
        }

        data = data.WithHash();
        return data.ETag.NotEmpty();
    }

    public async Task<Option> Append(string path, DataETag data, ScopeContext context)
    {
        if (!FileStoreTool.IsPathValid(path)) return StatusCode.BadRequest;
        context = context.With(_logger);
        string filePath = BuildFilePath(path);

        DataETag value = data;
        try
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                await fs.WriteAsync(data.Data.AsMemory());
            }

            context.LogTrace("Add Path={path} with eTag={eTag}, FilePath={filePath}", path, data.ETag, filePath);
        }
        catch (IOException ex)
        {
            return (StatusCode.Conflict, ex.Message);
        }

        return StatusCode.OK;
    }

    public async Task<Option> Delete(string path, ScopeContext context)
    {
        if (!FileStoreTool.IsPathValid(path)) return StatusCode.BadRequest;
        context = context.With(_logger);
        string filePath = BuildFilePath(path);

        await Task.Run(() => File.Delete(filePath));
        return StatusCode.OK;
    }

    public async Task<Option> Exist(string path, ScopeContext context)
    {
        if (!FileStoreTool.IsPathValid(path)) return StatusCode.BadRequest;
        context = context.With(_logger);
        string filePath = BuildFilePath(path);

        var exist = await Task.Run(() => File.Exists(filePath));
        return exist ? StatusCode.OK : StatusCode.NotFound;
    }

    public async Task<Option<DataETag>> Get(string path, ScopeContext context)
    {
        if (!FileStoreTool.IsPathValid(path)) return StatusCode.BadRequest;
        context = context.With(_logger);
        string filePath = BuildFilePath(path);

        try
        {
            if (!File.Exists(filePath)) return StatusCode.NotFound;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            {
                Memory<byte> buffer = new byte[fs.Length];
                await fs.ReadExactlyAsync(buffer);

                context.LogTrace("Get Path={path}, FilePath={filePath}", path, filePath);
                var data = buffer.ToDataETag();
                return data;
            }

        }
        catch (IOException ex)
        {
            return (StatusCode.Conflict, ex.Message);
        }
    }

    public Task<IReadOnlyList<string>> Search(string pattern, ScopeContext context)
    {
        var query = QueryParameter.Parse(pattern).GetMatcher();

        IReadOnlyList<string> list = Directory.GetFiles(_localFileStoreOption.BasePath, "*.*", SearchOption.AllDirectories)
            .Select(x => Path.GetRelativePath(_localFileStoreOption.BasePath, x))
            .Where(x => query.IsMatch(x, false))
            .ToImmutableArray();

        return list.ToTaskResult();
    }

    public async Task<Option<string>> Set(string path, DataETag data, ScopeContext context)
    {
        if (!FileStoreTool.IsPathValid(path)) return StatusCode.BadRequest;
        context = context.With(_logger);
        string filePath = BuildFilePath(path);

        try
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await fs.WriteAsync(data.Data.AsMemory());
            }

            context.LogTrace("Set Path={path} with eTag={eTag}, FilePath={filePath}", path, data.ETag, filePath);
        }
        catch (IOException ex)
        {
            return (StatusCode.Conflict, ex.Message);
        }

        data = data.WithHash();
        return data.ETag.NotEmpty();
    }

    private string BuildFilePath(string path)
    {
        var fullPath = Path.Combine(_localFileStoreOption.BasePath, path).Replace('/', '\\');
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath).NotNull());
        return fullPath;
    }
}

