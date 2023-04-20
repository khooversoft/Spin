using System.Collections;
using Toolbox.Tools;

namespace Toolbox.Types.MerkleTree;

public sealed class MerkleNode : IEnumerable<MerkleNode>
{
    public MerkleNode()
    {
    }

    /// <summary>
    /// Constructor for a base node (leaf), representing the lowest level of the tree.
    /// </summary>
    public MerkleNode(MerkleHash hash)
    {
        Hash = hash;
    }

    public MerkleHash Hash { get; private set; }

    public MerkleNode? LeftNode { get; private set; }

    public MerkleNode? RightNode { get; private set; }

    public MerkleNode? Parent { get; private set; }

    public bool IsLeaf { get { return LeftNode == null && RightNode == null; } }

    /// <summary>
    /// Constructor for a parent node.
    /// </summary>
    public MerkleNode(MerkleNode left, MerkleNode? right = null)
    {
        LeftNode = left;
        RightNode = right;
        LeftNode.Parent = this;

        if (RightNode != null) RightNode.Parent = this;
        ComputeHash();
    }

    public MerkleHash ComputeHash(byte[] buffer)
    {
        Hash = new MerkleHash(buffer);
        return Hash!;
    }

    public void SetLeftNode(MerkleNode node)
    {
        node.NotNull()
            .Assert(x => node.Hash != default, "Node hash must be initialized.");

        LeftNode = node;
        LeftNode.Parent = this;
        ComputeHash();
    }

    public void SetRightNode(MerkleNode node)
    {
        node.NotNull()
            .Assert(x => node.Hash != default, "Node hash must be initialized.");

        RightNode = node;
        RightNode.Parent = this;

        // Can't compute hash if the left node isn't set yet.
        if (LeftNode != null)
        {
            ComputeHash();
        }
    }

    /// <summary>
    /// True if we have enough data to verify our hash, particularly if we have child nodes.
    /// </summary>
    /// <returns>True if this node is a leaf or a branch with at least a left node.</returns>
    public bool CanVerifyHash()
    {
        return LeftNode != null && RightNode != null || LeftNode != null;
    }

    /// <summary>
    /// Verifies the hash for this node against the computed hash for our child nodes.
    /// If we don't have any children, the return is always true because we have nothing to verify against.
    /// </summary>
    public bool VerifyHash()
    {
        if (LeftNode == null && RightNode == null)
        {
            return true;
        }

        if (RightNode == null)
        {
            return Hash.Equals(LeftNode!.Hash);
        }

        LeftNode.Assert(x => x != null, "Left branch must be a node if right branch is a node.");
        MerkleHash leftRightHash = new MerkleHash(LeftNode!.Hash, RightNode.Hash);

        return Hash.Equals(leftRightHash);
    }

    public override string ToString() => Hash.ToString();

    /// <summary>
    /// If the hashes are equal, we know the entire node tree is equal.
    /// </summary>
    public bool Equals(MerkleNode node) => Hash.Equals(node.Hash);

    /// <summary>
    /// Return the leaves (not all children, just leaves) under this node
    /// </summary>
    public IEnumerable<MerkleNode> Leaves() => this.Where(n => n.LeftNode == null && n.RightNode == null);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<MerkleNode> GetEnumerator()
    {
        foreach (var n in Iterate(this)) yield return n;
    }

    /// <summary>
    /// Bottom-up/left-right iteration of the tree.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private IEnumerable<MerkleNode> Iterate(MerkleNode node)
    {
        if (node.LeftNode != null)
        {
            foreach (var n in Iterate(node.LeftNode)) yield return n;
        }

        if (node.RightNode != null)
        {
            foreach (var n in Iterate(node.RightNode)) yield return n;
        }

        yield return node;
    }

    private void ComputeHash()
    {
        // Repeat the left node if the right node doesn't exist.
        // This process breaks the case of doing a consistency check on 3 leaves when there are only 3 leaves in the tree.
        //MerkleHash rightHash = RightNode == null ? LeftNode.Hash : RightNode.Hash;
        //Hash = MerkleHash.Create(LeftNode.Hash.Value.Concat(rightHash.Value).ToArray());

        // Alternatively, do not repeat the left node, but carry the left node's hash up.
        // This process does not break the edge case described above.
        // We're implementing this version because the consistency check unit tests pass when we don't simulate
        // a right-hand node.

        Hash = RightNode == null ? LeftNode!.Hash : new MerkleHash(LeftNode!.Hash.Value.Concat(RightNode.Hash.Value).ToArray());

        Parent?.ComputeHash();      // Recurse, because out hash has changed.
    }
}