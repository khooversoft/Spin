﻿namespace Toolbox.Types;

public class MerkleProofHash
{
    public enum Branch
    {
        Left,
        Right,
        OldRoot,    // used for linear list of hashes to compute the old root in a consistency proof.
    }

    public MerkleHash Hash { get; protected set; }
    public Branch Direction { get; protected set; }

    public MerkleProofHash(MerkleHash hash, Branch direction)
    {
        Hash = hash;
        Direction = direction;
    }

    public override string ToString()
    {
        return Hash.ToString();
    }
}
