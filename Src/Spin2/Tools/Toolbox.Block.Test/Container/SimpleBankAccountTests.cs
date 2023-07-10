//using FluentAssertions;
//using Toolbox.Block.Container;
//using Toolbox.Block.Serialization;
//using Toolbox.Block.Signature;
//using Toolbox.Security.Principal;

//namespace Toolbox.Block.Test.Container;

//public class SimpleBankAccountTests
//{
//    private const string _contractServiceAccountKid = "contractService.kid";
//    private const string _contractServiceAccount = "contractService@domain.com";
//    private readonly PrincipalSignature _contractServiceSignature = new PrincipalSignature(_contractServiceAccountKid, _contractServiceAccount, "contractServices@domain.com");

//    private const string _contractOwnerAccountKid = "contractOwner.kid";
//    private const string _contractOwnerAccount = "contractOwner@domain.com";
//    private readonly PrincipalSignature _contractOwnerSignature = new PrincipalSignature(_contractOwnerAccountKid, _contractOwnerAccount, "contractServices@domain.com");

//    private const string _bankServiceAccountKid = "bankService.kid";
//    private const string _bankServiceAccount = "bankService@domain.com";
//    private readonly PrincipalSignature _bankServiceSignature = new PrincipalSignature(_bankServiceAccountKid, _bankServiceAccount, "contractServices@domain.com");

//    private readonly DateTime _date = DateTime.UtcNow;

//    [Fact]
//    public void GivenBlockChain_WhenContainered_ShouldRoundTrip()
//    {
//        SerializeProcess();
//    }

//    [Fact]
//    public void GivenAccountContract_SimulateTrx_ShouldPass()
//    {
//        BlockChain blockChain = SerializeProcess();

//        blockChain = AddAccount(blockChain);
//        blockChain.Blocks.Should().HaveCount(2);

//        AccountHeader header = blockChain.GetTypedBlocks<AccountHeader>().Single();
//        (header == new AccountHeader() with { AccountId = header.AccountId }).Should().BeTrue();

//        blockChain = Enumerable.Range(0, 5)
//            .Aggregate(blockChain, AddDetails);

//        blockChain.Validate(GetSignature);

//        blockChain.Blocks.Count.Should().Be(7);

//        IReadOnlyList<AccountDetail> details = blockChain.GetTypedBlocks<DataItemCollection<AccountDetail>>()
//            .SelectMany(x => x.Items)
//            .ToArray();

//        details.Count.Should().Be(10);
//    }

//    private BlockChain AddAccount(BlockChain blockChain)
//    {
//        var account = new AccountHeader();
//        blockChain.Add(account, _contractServiceAccountKid);

//        blockChain = blockChain.Sign(GetSignature);
//        blockChain.Validate(GetSignature);

//        return RoundTrip(blockChain);
//    }

//    private BlockChain AddDetails(BlockChain blockChain, int count)
//    {
//        int baseValue = count * 2;
//        var collection = new DataItemCollection<AccountDetail>()
//        {
//            Items = Enumerable.Range(baseValue, count)
//            .Select(x => new AccountDetail
//            {
//                Credit = false,
//                Amount = (baseValue + count) + 100.0m,
//            }).ToArray()
//        };

//        blockChain.Add(collection, _contractServiceAccountKid);

//        blockChain = blockChain.Sign(GetSignature);
//        blockChain.Validate(GetSignature);

//        return RoundTrip(blockChain);
//    }

//    private BlockChain CreateChain()
//    {
//        BlockChain blockChain = new BlockChainBuilder()
//            .SetPrincipleId(_contractServiceAccount)
//            .Build()
//            .Sign(x => _contractServiceSignature);

//        blockChain = blockChain.Sign(GetSignature);
//        blockChain.Validate(GetSignature);
//        return blockChain;
//    }

//    private BlockChain RoundTrip(BlockChain blockChain)
//    {
//        string blockChainHash = blockChain.ToMerkleTree().BuildTree().ToString();

//        byte[] blockChainData = blockChain
//            .ToBlockChainModel()
//            .ToPackage();

//        var readModel = blockChainData.ToBlockChainModel();

//        BlockChain result = readModel.ToBlockChain();

//        result.Validate(GetSignature);
//        string resultChainHash = result.ToMerkleTree().BuildTree().ToString();

//        blockChainHash.Should().Be(resultChainHash);

//        return result;
//    }


//    private BlockChain SerializeProcess()
//    {
//        BlockChain blockChain = CreateChain();
//        return RoundTrip(blockChain);
//    }

//    private PrincipalSignature GetSignature(string kid) => kid switch
//    {
//        _contractServiceAccountKid => _contractServiceSignature,
//        _contractOwnerAccountKid => _contractOwnerSignature,
//        _bankServiceAccountKid => _bankServiceSignature,

//        _ => throw new ArgumentException($"Invalid kid={kid}"),
//    };

//    private record AccountHeader
//    {
//        public string AccountId { get; init; } = Guid.NewGuid().ToString();
//        public string Name { get; init; } = "Account name";
//        public string? Email { get; init; }
//    }

//    public record AccountDetail
//    {
//        public Guid RowId { get; init; } = Guid.NewGuid();
//        public DateTime Date { get; init; } = DateTime.UtcNow;
//        public required bool Credit { get; init; }
//        public required decimal Amount { get; init; }
//    }
//}
