using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Security;
using Toolbox.Tools;

namespace Toolbox.BlockDocument.Block
{
    /// <summary>
    /// Block chain container
    /// </summary>
    public class BlockChain
    {
        private readonly List<BlockNode> _blocks;
        private readonly object _lock = new object();
        private CurrentDigest? _currentDigest = null;

        public BlockChain()
        {
            _blocks = new List<BlockNode>();
        }

        public BlockChain(IEnumerable<BlockNode> blockNodes)
        {
            blockNodes.VerifyNotNull(nameof(blockNodes));

            _blocks = blockNodes.ToList();
        }

        public IReadOnlyList<BlockNode> Blocks => _blocks;

        /// <summary>
        /// Get digest of block
        /// </summary>
        public string Digest
        {
            get
            {
                _currentDigest ??= new CurrentDigest(GetDigest(), _blocks.Count);

                if (((CurrentDigest)_currentDigest).BlockCount != _blocks.Count)
                {
                    _currentDigest = new CurrentDigest(GetDigest(), _blocks.Count);
                }

                return ((CurrentDigest)_currentDigest).Digest;
            }
        }

        /// <summary>
        /// Add data blocks
        /// </summary>
        /// <param name="dataBlocks"></param>
        public BlockChain Add(params DataBlock[] dataBlocks)
        {
            lock (_lock)
            {
                foreach (var item in dataBlocks)
                {
                    if( _blocks.Count == 0)
                    {
                        _blocks.Add(new BlockNode(item));
                        continue;
                    }

                    BlockNode latestBlock = Blocks[_blocks.Count - 1];

                    var newBlock = new BlockNode(item, latestBlock.Index + 1, latestBlock.Digest);
                    _blocks.Add(newBlock);
                }
            }

            return this;
        }

        public bool IsValid()
        {
            lock (_lock)
            {
                if (Blocks.Any(x => !x.IsValid())) return false;

                for (int i = 1; i < Blocks.Count; i++)
                {
                    BlockNode currentBlock = Blocks[i];
                    BlockNode previousBlock = Blocks[i - 1];

                    if (currentBlock.Digest != currentBlock.Digest) return false;
                    if (currentBlock.PreviousHash != previousBlock.Digest) return false;
                }

                return true;
            }
        }

        public string GetDigest()
        {
            lock (_lock)
            {
                return new MerkleTree()
                    .Append(_blocks.Select(x => new MerkleHash(x.GetDigest())).ToArray())
                    .BuildTree()
                    .ToString();
            }
        }

        public static BlockChain operator +(BlockChain self, DataBlock blockData)
        {
            self.Add(blockData);
            return self;
        }

        private struct CurrentDigest
        {
            public CurrentDigest(string digest, int blockCount)
            {
                Digest = digest;
                BlockCount = blockCount;
            }

            public string Digest { get; }

            public int BlockCount { get; }
        }
    }
}