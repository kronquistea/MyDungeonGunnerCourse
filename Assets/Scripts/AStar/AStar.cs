using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    /// <summary>
    /// Builds a path for the room, from the startGridPosition to the endGridPosition, and adds
    /// movement steps to the retuned stack. Returns null if no path is found.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="startGridPosition"></param>
    /// <param name="endGridPosition"></param>
    /// <returns>Stack of steps being the world positions for each grid square that the enemies should move between.</returns>
    public static Stack<Vector3> BuildPath(Room room, Vector3Int startGridPosition, Vector3Int endGridPosition)
    {
        // Adjust positions by lower bounds
        startGridPosition -= (Vector3Int)room.templateLowerBounds;
        endGridPosition -= (Vector3Int)room.templateLowerBounds;

        // Create open list and closed hashset
        BinaryHeap<Node> openNodeHeap = new BinaryHeap<Node>(room.templateUpperBounds.x * room.templateUpperBounds.y);
        HashSet<Node> closedNodeHashSet = new HashSet<Node>();

        // Create gridnodes for path finding
        GridNodes gridNodes = new GridNodes
            (
                room.templateUpperBounds.x - room.templateLowerBounds.x + 1,
                room.templateUpperBounds.y - room.templateLowerBounds.y + 1
            );

        Node startNode = gridNodes.GetGridNode(startGridPosition.x, startGridPosition.y);
        Node targetNode = gridNodes.GetGridNode(endGridPosition.x, endGridPosition.y);

        // Commence A* pathfinding algorithm
        Node endPathNode = FindShortestPath(startNode, targetNode, gridNodes, openNodeHeap, closedNodeHashSet, room.instantiatedRoom);

        // If a valid path was found, add the path to the path stack
        if (endPathNode != null)
        {
            return CreatePathStack(endPathNode, room);
        }

        // If not valid path was found, return null
        return null;
    }

    /// <summary>
    /// A* Pathfinding Algorithm
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="targetNode"></param>
    /// <param name="gridNodes"></param>
    /// <param name="openNodeHeap"></param>
    /// <param name="closedNodeHashSet"></param>
    /// <param name="instantiatedRoom"></param>
    /// <returns>Ending node for the path</returns>
    private static Node FindShortestPath(Node startNode, Node targetNode, GridNodes gridNodes,
      BinaryHeap<Node> openNodeHeap, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        // Add start node to open heap
        openNodeHeap.Add(startNode);

        // Loop through open node heap until empty
        while (openNodeHeap.Count > 0)
        {
            // current node = the node in the open heap with the lowest fCost
            Node currentNode = openNodeHeap.RemoveFirst();

            // If the current node == target node then finish
            if (currentNode == targetNode)
            {
                return currentNode;
            }

            // Add current node to the closed list 
            closedNodeHashSet.Add(currentNode);

            // Evaluate fCost for each neighbor of the current node
            EvaluateCurrentNodeNeighbors(currentNode, targetNode, gridNodes, openNodeHeap, closedNodeHashSet, instantiatedRoom);
        }

        // If no path was found, return null
        return null;
    }

    /// <summary>
    /// Create a stack of Vector3s containing the movement path
    /// </summary>
    /// <param name="targetNode"></param>
    /// <param name="room"></param>
    /// <returns>Stack of vector3s containing the movement path</returns>
    private static Stack<Vector3> CreatePathStack(Node targetNode, Room room)
    {
        Stack<Vector3> movementPathStack = new Stack<Vector3>();

        Node nextNode = targetNode;

        // Get midpoint of cell
        Vector3 cellMidPoint = room.instantiatedRoom.grid.cellSize * 0.5f;
        cellMidPoint.z = 0f;

        while (nextNode != null)
        {
            // Convert grid position to world position
            Vector3 worldPosition = room.instantiatedRoom.grid.CellToWorld(new Vector3Int(
              nextNode.gridPosition.x + room.templateLowerBounds.x, nextNode.gridPosition.y + room.templateLowerBounds.y, 0));

            // Set the world position to the middle of the grid cell
            worldPosition += cellMidPoint;

            movementPathStack.Push(worldPosition);

            nextNode = nextNode.parentNode;
        }

        return movementPathStack;
    }

    /// <summary>
    /// Check all neighboring nodes from current node and get distance costs
    /// </summary>
    /// <param name="currentNode"></param>
    /// <param name="targetNode"></param>
    /// <param name="gridNodes"></param>
    /// <param name="openNodeHeap"></param>
    /// <param name="closedNodeHashSet"></param>
    /// <param name="instantiatedRoom"></param>
    private static void EvaluateCurrentNodeNeighbors(Node currentNode, Node targetNode, GridNodes gridNodes,
      BinaryHeap<Node> openNodeHeap, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        Vector2Int currentNodeGridPositions = currentNode.gridPosition;
        Node validNeighborNode;

        // loop through all directions
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                validNeighborNode = GetValidNodeNeighbor(currentNodeGridPositions.x + i, currentNodeGridPositions.y + j, gridNodes, closedNodeHashSet, instantiatedRoom);

                if (validNeighborNode != null)
                {
                    // Calculate new gCost for neighbor
                    int newCostToNeighbor;

                    // Get the movement penalty
                    int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[validNeighborNode.gridPosition.x, validNeighborNode.gridPosition.y];

                    newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, validNeighborNode) + movementPenaltyForGridSpace;

                    bool isValidNeighborNodeInOpenHeap = openNodeHeap.Contains(validNeighborNode);

                    if (newCostToNeighbor < validNeighborNode.gCost || !isValidNeighborNodeInOpenHeap)
                    {
                        validNeighborNode.gCost = newCostToNeighbor;
                        validNeighborNode.hCost = GetDistance(validNeighborNode, targetNode);
                        validNeighborNode.parentNode = currentNode;

                        if (!isValidNeighborNodeInOpenHeap)
                        {
                            openNodeHeap.Add(validNeighborNode);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calculate the distance between nodeA and nodeB
    /// </summary>
    /// <param name="nodeA"></param>
    /// <param name="nodeB"></param>
    /// <returns>An integer representing the distance between nodeA and nodeB</returns>
    private static int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        // 10 is used for each unit and 14 is a diagonal approximation.
        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }

        return 14 * dstX + 10 * (dstY - dstX);
    }

    /// <summary>
    /// Evaluate a neighbor node at neighborNodeXPosition, neighborNodeYPosition, using the specified gridNodes, closedNodeHashSet, and instantiated room.
    /// </summary>
    /// <param name="neighborNodeXPosition"></param>
    /// <param name="neighborNodeYPosition"></param>
    /// <param name="gridNodes"></param>
    /// <param name="closedNodeHashSet"></param>
    /// <param name="instantiatedRoom"></param>
    /// <returns>Valid neighbor node if node is valid, null otherwise</returns>
    private static Node GetValidNodeNeighbor(int neighborNodeXPosition, int neighborNodeYPosition, GridNodes gridNodes,
      HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        // If neighbor node position is beyond grid then return null
        if (neighborNodeXPosition >= instantiatedRoom.room.templateUpperBounds.x - instantiatedRoom.room.templateLowerBounds.x ||
            neighborNodeXPosition < 0 || 
            neighborNodeYPosition >= instantiatedRoom.room.templateUpperBounds.y - instantiatedRoom.room.templateLowerBounds.y ||
            neighborNodeYPosition < 0)
        {
            return null;
        }

        // get Neighbor node
        Node neighborNode = gridNodes.GetGridNode(neighborNodeXPosition, neighborNodeYPosition);

        // Check for obstacle
        int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[neighborNodeXPosition, neighborNodeYPosition];

        // If neighbor is in the closed list then skip or if is an obstacle
        if (movementPenaltyForGridSpace == 0 || closedNodeHashSet.Contains(neighborNode))
        {
            return null;
        }

        return neighborNode;
    }
}