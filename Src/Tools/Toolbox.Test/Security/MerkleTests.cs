using System.Text;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Test.Security
{
    public class MerkleTests
    {
        [Fact]
        public void HashesAreSameTest()
        {
            MerkleHash h1 = new MerkleHash("abc");
            MerkleHash h2 = new MerkleHash("abc");
            (h1 == h2).Should().BeTrue();
        }

        [Fact]
        public void CreateNodeTest()
        {
            MerkleNode node = new MerkleNode();
            node.Parent.BeNull();
            node.LeftNode.BeNull();
            node.RightNode.BeNull();
        }

        /// <summary>
        /// Tests that after setting the left node, the parent hash verifies.
        /// </summary>
        [Fact]
        public void LeftHashVerificationTest()
        {
            MerkleNode parentNode = new MerkleNode();
            MerkleNode leftNode = new MerkleNode();
            leftNode.ComputeHash(Encoding.UTF8.GetBytes("abc"));
            parentNode.SetLeftNode(leftNode);
            parentNode.VerifyHash().Should().BeTrue();
        }

        /// <summary>
        /// Tests that after setting both child nodes (left and right), the parent hash verifies.
        /// </summary>
        [Fact]
        public void LeftRightHashVerificationTest()
        {
            MerkleNode parentNode = CreateParentNode("abc", "def");
            parentNode.VerifyHash().Should().BeTrue();
        }

        [Fact]
        public void NodesEqualTest()
        {
            MerkleNode parentNode1 = CreateParentNode("abc", "def");
            MerkleNode parentNode2 = CreateParentNode("abc", "def");
            parentNode1.Equals(parentNode2).Should().BeTrue();
        }

        [Fact]
        public void NodesNotEqualTest()
        {
            MerkleNode parentNode1 = CreateParentNode("abc", "def");
            MerkleNode parentNode2 = CreateParentNode("def", "abc");
            parentNode1.Equals(parentNode2).Should().BeFalse();
        }

        [Fact]
        public void VerifyTwoLevelTree()
        {
            MerkleNode parentNode1 = CreateParentNode("abc", "def");
            MerkleNode parentNode2 = CreateParentNode("123", "456");
            MerkleNode rootNode = new MerkleNode();
            rootNode.SetLeftNode(parentNode1);
            rootNode.SetRightNode(parentNode2);
            rootNode.VerifyHash().Should().BeTrue();
        }

        [Fact]
        public void CreateBalancedTreeTest()
        {
            MerkleTree tree = new MerkleTree();
            tree.Append(new MerkleHash("abc"));
            tree.Append(new MerkleHash("def"));
            tree.Append(new MerkleHash("123"));
            tree.Append(new MerkleHash("456"));
            tree.BuildTree();
            tree.RootNode.NotNull();
        }

        [Fact]
        public void CreateUnbalancedTreeTest()
        {
            MerkleTree tree = new MerkleTree();
            tree.Append(new MerkleHash("abc"));
            tree.Append(new MerkleHash("def"));
            tree.Append(new MerkleHash("123"));
            tree.BuildTree();
            tree.RootNode.NotNull();
        }

        // A Merkle audit path for a leaf in a Merkle Hash Tree is the shortest
        // list of additional nodes in the Merkle Tree required to compute the
        // Merkle Tree Hash for that tree.
        [Fact]
        public void AuditTest()
        {
            // Build a tree, and given the root node and a leaf hash, verify that the we can reconstruct the root hash.
            MerkleTree tree = new MerkleTree();
            MerkleHash l1 = new MerkleHash("abc");
            MerkleHash l2 = new MerkleHash("def");
            MerkleHash l3 = new MerkleHash("123");
            MerkleHash l4 = new MerkleHash("456");
            tree.Append(new MerkleHash[] { l1, l2, l3, l4 });
            MerkleHash rootHash = tree.BuildTree();

            List<MerkleProofHash> auditTrail = tree.AuditProof(l1);
            MerkleTree.VerifyAudit(rootHash, l1, auditTrail).Should().BeTrue();

            auditTrail = tree.AuditProof(l2);
            MerkleTree.VerifyAudit(rootHash, l2, auditTrail).Should().BeTrue();

            auditTrail = tree.AuditProof(l3);
            MerkleTree.VerifyAudit(rootHash, l3, auditTrail).Should().BeTrue();

            auditTrail = tree.AuditProof(l4);
            MerkleTree.VerifyAudit(rootHash, l4, auditTrail).Should().BeTrue();
        }

        [Fact]
        public void FixingOddNumberOfLeavesByAddingTreeTest()
        {
            MerkleTree tree = new MerkleTree();
            MerkleHash l1 = new MerkleHash("abc");
            MerkleHash l2 = new MerkleHash("def");
            MerkleHash l3 = new MerkleHash("123");
            tree.Append(new MerkleHash[] { l1, l2, l3 });
            MerkleHash rootHash = tree.BuildTree();
            tree.AddTree(new MerkleTree());
            MerkleHash rootHashAfterFix = tree.BuildTree();
            (rootHash == rootHashAfterFix).Should().BeTrue();
        }

        [Fact]
        public void FixingOddNumberOfLeavesManuallyTest()
        {
            MerkleTree tree = new MerkleTree();
            MerkleHash l1 = new MerkleHash("abc");
            MerkleHash l2 = new MerkleHash("def");
            MerkleHash l3 = new MerkleHash("123");
            tree.Append(new MerkleHash[] { l1, l2, l3 });
            MerkleHash rootHash = tree.BuildTree();
            tree.FixOddNumberLeaves();
            MerkleHash rootHashAfterFix = tree.BuildTree();
            (rootHash != rootHashAfterFix).Should().BeTrue();
        }

        [Fact]
        public void AddTreeTest()
        {
            MerkleTree tree = new MerkleTree();
            MerkleHash l1 = new MerkleHash("abc");
            MerkleHash l2 = new MerkleHash("def");
            MerkleHash l3 = new MerkleHash("123");
            tree.Append(new MerkleHash[] { l1, l2, l3 });
            MerkleHash rootHash = tree.BuildTree();

            MerkleTree tree2 = new MerkleTree();
            MerkleHash l5 = new MerkleHash("456");
            MerkleHash l6 = new MerkleHash("xyzzy");
            MerkleHash l7 = new MerkleHash("fizbin");
            MerkleHash l8 = new MerkleHash("foobar");
            tree2.Append(new MerkleHash[] { l5, l6, l7, l8 });
            MerkleHash tree2RootHash = tree2.BuildTree();
            MerkleHash rootHashAfterAddTree = tree.AddTree(tree2);

            (rootHash != rootHashAfterAddTree).Should().BeTrue();
        }

        // Merkle consistency proofs prove the append-only property of the tree.
        [Fact]
        public void ConsistencyTest()
        {
            // Start with a tree with 2 leaves:
            MerkleTree tree = new MerkleTree();
            var startingNodes = tree.Append(new MerkleHash[]
                {
                    new MerkleHash("1"),
                    new MerkleHash("2"),
                });

            // startingNodes.ForEachWithIndex((n, i) => n.Text = i.ToString());

            MerkleHash firstRoot = tree.BuildTree();

            List<MerkleHash> oldRoots = new List<MerkleHash>() { firstRoot };

            // Add a new leaf and verify that each time we add a leaf, we can get a consistency check
            // for all the previous leaves.
            for (int i = 2; i < 100; i++)
            {
                tree.Append(new MerkleHash(i.ToString())); //.Text=i.ToString();
                tree.BuildTree();

                // After adding a leaf, verify that all the old root hashes exist.
                oldRoots.ForEach((oldRootHash, n) =>
                {
                    List<MerkleProofHash> proof = tree.ConsistencyProof(n + 2);
                    MerkleHash hash, lhash, rhash;

                    if (proof.Count > 1)
                    {
                        lhash = proof[proof.Count - 2].Hash;
                        int hidx = proof.Count - 1;
                        hash = rhash = MerkleTree.ComputeHash(lhash, proof[hidx].Hash);
                        hidx -= 2;

                        while (hidx >= 0)
                        {
                            lhash = proof[hidx].Hash;
                            hash = rhash = MerkleTree.ComputeHash(lhash, rhash);

                            --hidx;
                        }
                    }
                    else
                    {
                        hash = proof[0].Hash;
                    }

                    (hash == oldRootHash).Should().BeTrue("Old root hash not found for index " + i + " m = " + (n + 2).ToString());
                });

                // Then we add this root hash as the next old root hash to check.
                oldRoots.Add(tree.RootNode!.Hash);
            }
        }

        private MerkleNode CreateParentNode(string leftData, string rightData)
        {
            MerkleNode parentNode = new MerkleNode();
            MerkleNode leftNode = new MerkleNode();
            MerkleNode rightNode = new MerkleNode();
            leftNode.ComputeHash(Encoding.UTF8.GetBytes(leftData));
            rightNode.ComputeHash(Encoding.UTF8.GetBytes(rightData));
            parentNode.SetLeftNode(leftNode);
            parentNode.SetRightNode(rightNode);

            return parentNode;
        }
    }
}