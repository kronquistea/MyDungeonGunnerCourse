using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    /// <summary>
    /// Builds a path for the room, from the startGridPosition to the endGridPosition, and adds
    /// movement steps to the returned stack. Returns null if no path is found.
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
        List<Node> openNodeList = new List<Node>();
        HashSet<Node> closedNodeHashSet = new HashSet<Node>();

        // Create gridnodes for path finding
        GridNodes gridNodes = new GridNodes
            (
                room.templateUpperBounds.x - room.templateLowerBounds.x + 1, 
                room.templateUpperBounds.y - room.templateLowerBounds.y + 1
            );

        Node startNode = gridNodes.GetGridNode(startGridPosition.x, startGridPosition.y);
        Node endNode = gridNodes.GetGridNode(endGridPosition.x, endGridPosition.y);

        // Commence A* pathfinding algorithm
        Node endPathNode = FindShortestPath(startNode, endNode, gridNodes, openNodeList, closedNodeHashSet, room.instantiatedRoom);

        // If a valid path was found, add the path to the path stack
        if (endPathNode != null)
        {
            return CreatePathStack(endPathNode, room);
        }

        // If no valid path was found, return null
        return null;
    }

    /// <summary>
    /// A Start Pathfinding Algorithm
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    /// <param name="gridNodes"></param>
    /// <param name="openNodeList"></param>
    /// <param name="closedNodeHashSet"></param>
    /// <param name="instantiatedRoom"></param>
    /// <returns></returns>
    private static Node FindShortestPath(Node startNode, Node endNode, GridNodes gridNodes, List<Node> openNodeList, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        openNodeList.Add(startNode);

        // Loop until there are no more open nodes to evaluate (or path is already found)
        while (openNodeList.Count > 0)
        {
            // Sort the list of open nodes
            openNodeList.Sort();

            // The current node is the first node in the open list
            Node currentNode = openNodeList[0];
            openNodeList.RemoveAt(0);

            // If thd current node is the ending node, return the current node and stop looping
            if (currentNode == endNode)
            {
                return currentNode;
            }

            // Add the current node to the hast set of closed nodes
            closedNodeHashSet.Add(currentNode);

            // Evaluate all the neighbors (orthogonally and diagonally) from the current node
            EvaluateCurrentNodeNeighbors(currentNode, endNode, gridNodes, openNodeList, closedNodeHashSet, instantiatedRoom);
        }

        // Haven't found a path so return null
        return null;
    }

    /// <summary>
    /// Check all neighboring nodes from current node and get distance costs
    /// </summary>
    /// <param name="currentNode"></param>
    /// <param name="endNode"></param>
    /// <param name="gridNodes"></param>
    /// <param name="openNodeList"></param>
    /// <param name="closedNodeHashSet"></param>
    /// <param name="instantiatedRoom"></param>
    private static void EvaluateCurrentNodeNeighbors(Node currentNode, Node endNode, GridNodes gridNodes, List<Node> openNodeList, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        // Store the current position of the node in the grid
        Vector2Int currentNodeGridPosition = currentNode.gridPosition;

        Node validNeighborNode;

        // Loop through all surrounding nodes (diagonally and orthogonally)
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                // If the node is the starting node, skip it
                if (i == 0 && j == 0)
                {
                    continue;
                }

                // Get the neighbor node that is currently being checked by the for loop
                validNeighborNode = GetValidNodeNeighbor(currentNodeGridPosition.x + i, currentNodeGridPosition.y + j, gridNodes, closedNodeHashSet, instantiatedRoom);
            
                // If the node actually exists (not out of bounds, not already checked, and not an obstacle), process the node
                if (validNeighborNode != null)
                {
                    int newCostToNeighbor;

                    // Get the movement penality. Unwalkable paths have a value of 0. Default movement penality is set
                    // in Settings (script) and applies to other grid squares.
                    int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[validNeighborNode.gridPosition.x, validNeighborNode.gridPosition.y];

                    // Neighbor gCost is currentNode gCost + cost from current node to neighbor node
                    newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, validNeighborNode) + movementPenaltyForGridSpace;

                    // Check if the openNodeList has the neighbor node
                    bool isValidNeighborNodeInOpenList = openNodeList.Contains(validNeighborNode);

                    // If the new cost is less than the old cost or the neighbor is not already in the openList, process it
                    if (newCostToNeighbor < validNeighborNode.gCost || !isValidNeighborNodeInOpenList)
                    {
                        // Set the new gCost
                        validNeighborNode.gCost = newCostToNeighbor;
                        // Set the hCost based on distance from the node to the end node
                        validNeighborNode.hCost = GetDistance(validNeighborNode, endNode);
                        // Set the parent node of the (neighbor) node to the current node
                        validNeighborNode.parentNode = currentNode;

                        // If the node is not yet in the open list, add it to the open list
                        if (!isValidNeighborNodeInOpenList)
                        {
                            openNodeList.Add(validNeighborNode);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Evaluate a neighbor node at neighborNodeXPosition, neighborNodeYPosition, using the specified gridNodes, closedNodeHashSet, and instantiated room.
    /// </summary>
    /// <param name="neighborNodeXPosition"></param>
    /// <param name="neighborNodeYPosition"></param>
    /// <param name="gridNodes"></param>
    /// <param name="closedNodeHashSet"></param>
    /// <param name="instantiatedRoom"></param>
    /// <returns>Node if node is valid, null otherwise</returns>
    private static Node GetValidNodeNeighbor(int neighborNodeXPosition, int neighborNodeYPosition, GridNodes gridNodes, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        // If the neighbor node position is out of the bounds of the room, return null
        if (
            neighborNodeXPosition >= instantiatedRoom.room.templateUpperBounds.x - instantiatedRoom.room.templateLowerBounds.x ||
            neighborNodeXPosition < 0 ||
            neighborNodeYPosition >= instantiatedRoom.room.templateUpperBounds.y - instantiatedRoom.room.templateLowerBounds.y ||
            neighborNodeYPosition < 0
            )
        {
            return null;
        }

        // Get the neighbor node at the coordinate pair
        Node neighborNode = gridNodes.GetGridNode(neighborNodeXPosition, neighborNodeYPosition);

        int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[neighborNodeXPosition, neighborNodeYPosition];

        // If the neighbor node is already in the closed set or neighbor is an obstacle then skip
        if (movementPenaltyForGridSpace == 0 || closedNodeHashSet.Contains(neighborNode))
        {
            return null;
        }

        // Return the neighbor node as all checks were passed. 
        return neighborNode;
    }

    /// <summary>
    /// Calculate the distance between nodeA and nodeB
    /// </summary>
    /// <param name="nodeA"></param>
    /// <param name="nodeB"></param>
    /// <returns>An integer representing the distance between nodeA and nodeB</returns>
    private static int GetDistance(Node nodeA, Node nodeB)
    {
        // Get x and y distanced between nodes A and B
        int dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        // 14 is used as the diagonal distance - because of SQRT(10^2 + 10^2) (pythagorean theorem), 10 is used as the orthogonal distance
        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        else
        {
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }

    /// <summary>
    /// Create a stack of vector3s containing the movement path
    /// </summary>
    /// <param name="endNode"></param>
    /// <param name="room"></param>
    /// <returns>Stack of vector3s containing the movement path</returns>
    private static Stack<Vector3> CreatePathStack(Node endNode, Room room)
    {
        // Stack of world positions
        Stack<Vector3> movementPathStack = new Stack<Vector3>();

        Node nextNode = endNode;

        // Calculate the mid point of a grid square (to be used as an "offset" for the calculated world position
        Vector3 cellMidPoint = room.instantiatedRoom.grid.cellSize * 0.5f;
        cellMidPoint.z = 0f;

        // Loop until the path to the starting node is found
        while (nextNode != null)
        {
            // Convert grid position to world position
            Vector3 worldPosition = room.instantiatedRoom.grid.CellToWorld(
                new Vector3Int
                (
                    nextNode.gridPosition.x + room.templateLowerBounds.x, 
                    nextNode.gridPosition.y + room.templateLowerBounds.y, 
                    0
                )
            );

            // Set the world position to the middle of the grid cell (as the pivot point of grid cells is located in the bottom left
            // - but we want to set the world position to the center of the grid cell
            worldPosition += cellMidPoint;

            movementPathStack.Push(worldPosition);

            nextNode = nextNode.parentNode;
        }

        return movementPathStack;
    }
}
