using Contract.sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Contract.sdk.Container;

public class ContractContainer
{
    private readonly BlockChain _blockChain;

    public ContractContainer(string issuer)
    {
        issuer.NotEmpty();

        _blockChain = new BlockChainBuilder()
            .SetPrincipleId(issuer)
            .Build();
    }

    public ContractContainer(BlockChain blockChain) => _blockChain = blockChain.NotNull();

    public ContractDocumentModel Get()
    {
        ContractBlkHeader header = _blockChain.FindBlockType<ContractBlkHeader>()
            .FirstOrDefault()
            .NotNull(name: $"Cannot find {nameof(ContractBlkHeader)}");

        IReadOnlyList<ContractBlkGroup> groups = _blockChain.FindBlockType<ContractBlkGroup>();

        return header.ConvertTo(groups);
    }

    public void Set(ContractDocumentModel model)
    {
        model.NotNull();

        ContractBlkHeader? header = _blockChain.FindBlockType<ContractBlkHeader>().FirstOrDefault();
        IReadOnlyList<ContractBlkGroup> groups = _blockChain.FindBlockType<ContractBlkGroup>();

        if (header == null)
        {
            var dataBlockHeader = new DataBlockBuilder()
                .SetTimeStamp(model.Date)
                .SetBlockType<ContractBlkHeader>()
                .SetBlockId(Guid.NewGuid().ToString())
                .SetData(model.ConvertTo())
                .SetPrincipleId(model.PrincipleId)
                .Build();

            _blockChain.Add(dataBlockHeader);
        }

        model.NewGroups.Select(x => new DataBlockBuilder()
            .SetTimeStamp(x.Date)
            .SetBlockType<ContractBlkGroup>()
            .SetBlockId(Guid.NewGuid().ToString())
            .SetData(x.ConvertTo())
            .SetPrincipleId(model.PrincipleId)
            .Build()
            ).ForEach(x => _blockChain.Add(x));
    }
}
