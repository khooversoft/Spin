using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Block.Application;
using Toolbox.Block.Container;
using Toolbox.Block.Signature;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block.Stream;


public enum StreamType
{
    Snapshot,
    Ledger
}

public static class StreamTypeExtensions
{
    public static string ToBlockType(this StreamType subject, string streamName) => $"{subject}:{streamName.NotEmpty()}";
}

/// <summary>
/// Provides multiple stream capability for block chains.
/// 
/// BlockType = resource path : "{streamName}"
/// ObjectType = object type name (serialize / deserialize)
/// 
/// </summary>
public class BlockStream
{
    private readonly BlockChain _blockChain;
    private readonly string _streamName;
    private readonly string _blockType;

    public BlockStream(BlockChain blockChain, string streamName, StreamType streamType)
    {
        _blockChain = blockChain.NotNull();
        _streamName = streamName.NotNull();
        _blockType = streamType.ToBlockType(streamName);
        StreamType = streamType;
    }

    public StreamType StreamType { get; }

    public BlockStream Add<T>(T value, string principleId) where T : class
    {
        _blockChain.Add(value, principleId, _blockType);
        return this;
    }

    public Option<T> Get<T>() where T : class => _blockChain
        .GetTypedBlocks<T>(_blockType)
        .FirstOrDefault()
        .ToOption();

    //public Option<IReadOnlyList<
}
