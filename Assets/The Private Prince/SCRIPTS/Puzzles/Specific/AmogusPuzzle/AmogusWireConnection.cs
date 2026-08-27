using UnityEngine;

public class AmogusWireConnection
{
    public AmogusWireNode leftNode;
    public AmogusWireNode rightNode;
    public AmogusWireUI line;

    public AmogusWireConnection(
        AmogusWireNode left,
        AmogusWireNode right,
        AmogusWireUI wire)
    {
        leftNode = left;
        rightNode = right;
        line = wire;
    }
}