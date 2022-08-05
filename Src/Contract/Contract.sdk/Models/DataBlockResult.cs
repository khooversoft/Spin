using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Abstractions;
using Toolbox.Tools;

namespace Contract.sdk.Models;

public record DataBlockResult
{
    public DataBlockResult(string blockType, Document document)
    {
        BlockType = blockType.NotEmpty();
        Document = document.NotNull(); ;
    }

    public string BlockType { get; }
    public Document Document { get; }
}
