﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading.Tasks.Dataflow;
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

    public static async Task<Option<IReadOnlyList<FileHash>>> GetFileHashes(IEnumerable<string> files, ScopeContext context)
    {
        context.Trace().LogInformation("Calculating hash for all files");

        var fileList = files.ToArray();
        var errorCnt = 0;
        var queue = new ConcurrentQueue<FileHash>();
        var block = new ActionBlock<string>(getFileHash, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 5
        });

        fileList.ForEach(x => block.Post(x));
        block.Complete();
        await block.Completion;

        var diffTest = fileList.Length - queue.Count;
        if (diffTest != 0) throw new InvalidOperationException($"diffTest violation, fileCount={fileList.Length}, processedCount={queue.Count}");

        return queue.OrderBy(x => x.File).ToArray();

        async Task getFileHash(string file)
        {
            var result = await GetFileHash(file, context);
            if (result.IsError())
            {
                context.Location().LogError("Failed to get hash of file, file={file}", file);
                errorCnt++;
                return;
            }

            queue.Enqueue(result.Return());
        }
    }

    public static Task<Option<FileHash>> GetFileHash(string file, ScopeContext context)
    {
        if (!File.Exists(file)) return new Option<FileHash>(StatusCode.NotFound).ToTaskResult();

        using FileStream fileStream = File.OpenRead(file);
        using SHA256 hasher = SHA256.Create();

        try
        {
            fileStream.Position = 0;

            // Compute the hash of the fileStream.
            byte[] hashValue = hasher.ComputeHash(fileStream);

            return new FileHash
            {
                File = file,
                Hash = hashValue.ToHex(),
            }.ToOption().ToTaskResult();
        }
        catch (IOException ex)
        {
            context.Location().LogError(ex, "Failed to hash file={file}", file);
            return new Option<FileHash>(StatusCode.InternalServerError).ToTaskResult();
        }
        catch (UnauthorizedAccessException ex)
        {
            context.Location().LogError(ex, "Access Exception for hash file={file}", file);
            return new Option<FileHash>(StatusCode.InternalServerError).ToTaskResult();
        }
    }
}
