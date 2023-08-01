using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace Toolbox.Block.Test;

public class BlockAccessTests
{
    [Fact]
    public void GrantAccessMaskTests()
    {
        ((BlockGrant.None & BlockGrant.None) == BlockGrant.None).Should().BeTrue();
        (BlockGrant.None & BlockGrant.None).HasFlag(BlockGrant.None).Should().BeTrue();

        ((BlockGrant.Read & BlockGrant.Read) == BlockGrant.Read).Should().BeTrue();
        ((BlockGrant.Write & BlockGrant.Write) == BlockGrant.Write).Should().BeTrue();

        (((BlockGrant.None | BlockGrant.Read) & BlockGrant.None) == BlockGrant.None).Should().BeTrue();
        ((BlockGrant.None & BlockGrant.Read) == BlockGrant.None).Should().BeFalse();

        (((BlockGrant.None | BlockGrant.Read) & BlockGrant.Read) == BlockGrant.Read).Should().BeTrue();
        (((BlockGrant.Read | BlockGrant.Write) & BlockGrant.Read) == BlockGrant.Read).Should().BeTrue();
        (BlockGrant.Read | BlockGrant.Write).HasFlag(BlockGrant.Write).Should().BeTrue();
    }


    [Theory]
    [InlineData(BlockGrant.None, "blockType", "principlayId", BlockGrant.None, "blockType", "principlayId")]
    [InlineData(BlockGrant.Read, "blockType2", "principlayId2", BlockGrant.Read, "blockType2", "principlayId2")]
    [InlineData(BlockGrant.Write, "blockType3", "principlayId3", BlockGrant.Write, "blockType3", "principlayId3")]
    public void TestEqual(BlockGrant grant, string blockType, string principalId, BlockGrant grant2, string blockType2, string principalId2)
    {
        var v1 = new BlockAccess { Grant = grant, BlockType = blockType, PrincipalId = principalId };
        var v2 = new BlockAccess { Grant = grant2, BlockType = blockType2, PrincipalId = principalId2 };
        (v1 == v2).Should().BeTrue();
        (v1 != v2).Should().BeFalse();
    }
    
    [Theory]
    [InlineData(BlockGrant.None, "blockType", "principlayId", BlockGrant.None | BlockGrant.Read, "blockType", "principlayId")]
    [InlineData(BlockGrant.Read, "blockType2", "principlayId2", BlockGrant.None, "blockType2", "principlayId2")]
    [InlineData(BlockGrant.Write, "blockType3", "principlayId3", BlockGrant.Write, "blockType3", "principlayId3")]
    public void TestMaskEqual(BlockGrant grant, string blockType, string principalId, BlockGrant grant2, string blockType2, string principalId2)
    {
        var v1 = new BlockAccess { Grant = grant, BlockType = blockType, PrincipalId = principalId };
        var v2 = new BlockAccess { Grant = grant2, BlockType = blockType2, PrincipalId = principalId2 };
        (v1 == v2).Should().BeTrue();
        (v1 != v2).Should().BeFalse();
    }
    
    [Theory]
    [InlineData(BlockGrant.None, "blockType", "principlayId", BlockGrant.Write, "blockType", "principlayId")]
    [InlineData(BlockGrant.Read, "blockType2", "principlayId2", BlockGrant.None, "blockType2", "principlayId2")]
    [InlineData(BlockGrant.Write, "blockType3", "principlayId3", BlockGrant.Read, "blockType3", "principlayId3")]
    public void TestNotEqual(BlockGrant grant, string blockType, string principalId, BlockGrant grant2, string blockType2, string principalId2)
    {
        var v1 = new BlockAccess { Grant = grant, BlockType = blockType, PrincipalId = principalId };
        var v2 = new BlockAccess { Grant = grant2, BlockType = blockType2, PrincipalId = principalId2 };
        (v1 == v2).Should().BeFalse();
        (v1 != v2).Should().BeTrue();
    }
}
