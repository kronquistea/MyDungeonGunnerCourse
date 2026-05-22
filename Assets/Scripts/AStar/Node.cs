using UnityEngine;

public class Node : IHeapItem<Node>
{
    public Vector2Int gridPosition;
    public int gCost;
    public int hCost;
    public Node parentNode;
    public int HeapIndex { get; set; }

    public int fCost
    {
        get { return gCost + hCost; }
    }

    public Node(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }

        return -compare;
    }
}