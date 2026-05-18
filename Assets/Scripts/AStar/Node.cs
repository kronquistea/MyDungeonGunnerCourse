using System;
using UnityEngine;

public class Node : IComparable<Node>
{
    public Vector2Int gridPosition;
    public int gCost = 0; // Distance from starting node
    public int hCost = 0; // Distance from ending node
    public Node parentNode;

    public Node(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;
        parentNode = null;
    }

    public int FCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        // Compare will be < 0 if this instance FCost is less than nodeToCompare.FCost
        // Compare will be > 0 if this instance FCost is greater than nodeToCompare.FCost
        // Compare will be = 0 if this instance FCost is equal to nodeToCompare.FCost

        int compare = FCost.CompareTo(nodeToCompare.FCost);

        // Sort by hCost if FCost's are the same
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }

        return compare;
    }
}
