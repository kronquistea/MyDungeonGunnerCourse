using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarTest : MonoBehaviour
{
    private InstantiatedRoom instantiatedRoom;
    private Grid grid;
    private Tilemap frontTilemap;
    private Tilemap pathTilemap;
    private Vector3Int startGridPosition;
    private Vector3Int endGridPosition;
    private TileBase startPathTile;
    private TileBase finishPathTile;

    private Vector3Int noValue = new Vector3Int(9999, 9999, 9999);
    private Stack<Vector3> pathStack;

    private void OnEnable()
    {
        // Subscribe to the onRoomChanged event
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from the onRoomChanged Event
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    private void Start()
    {
        startPathTile = GameResources.Instance.preferredEnemyTilePath;
        finishPathTile = GameResources.Instance.enemyUnwalkableCollisionTilesArray[0];
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        pathStack = null;
        instantiatedRoom = roomChangedEventArgs.room.instantiatedRoom;
        frontTilemap = instantiatedRoom.transform.Find("Grid/Tilemap4_Front").GetComponent<Tilemap>();
        grid = instantiatedRoom.transform.GetComponentInChildren<Grid>();
        startGridPosition = noValue;
        endGridPosition = noValue;

        SetUpPathTilemap();
    }

    /// <summary>
    /// Use a clone of the front tilemap for the path tilemap.
    /// If not created then create one, otherwise use the existing one (don't want to remake tons of tilemaps).
    /// </summary>
    private void SetUpPathTilemap()
    {
        Transform tilemapCloneTransform = instantiatedRoom.transform.Find("Grid/Tilemap4_Front(Clone)");

        // If the front tilemap hasn't been cloned then clone it
        if (tilemapCloneTransform == null)
        {
            pathTilemap = Instantiate(frontTilemap, grid.transform);
            pathTilemap.GetComponent<TilemapRenderer>().sortingOrder = 2; // Appear in front of frontTilemap
            pathTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
            pathTilemap.gameObject.tag = "Untagged";
        }
        else
        {
            pathTilemap = instantiatedRoom.transform.Find("Grid/Tilemap4_Front(Clone)").GetComponent<Tilemap>();
            pathTilemap.ClearAllTiles();
        }
    }

    private void Update()
    {
        // Validation checks
        if (instantiatedRoom == null || startPathTile == null || finishPathTile == null || grid == null || pathTilemap == null)
        {
            return;
        }
        
        // Start position key
        if (Input.GetKeyDown(KeyCode.I))
        {
            ClearPath();
            SetStartPosition();
        }

        // End position key
        if (Input.GetKeyDown(KeyCode.O))
        {
            ClearPath();
            SetEndPosition();
        }

        // Display path key
        if (Input.GetKeyDown(KeyCode.P))
        {
            DisplayPath();
        }
    }

    /// <summary>
    /// Set the start position and the start tile on the front tilemap
    /// </summary>
    private void SetStartPosition()
    {
        // Check if the start position has not been populated yet
        if (startGridPosition == noValue)
        {
            // Set the start position to the cell the mouse is hovering over
            startGridPosition = grid.WorldToCell(HelperUtilities.GetMouseWorldPosition());

            // If the position is not within the bounds of the tilemap, do nothing
            if (!IsPositionWithinBounds(startGridPosition))
            {
                startGridPosition = noValue;
                return;
            }

            // Set the start path tile at the mouse position
            pathTilemap.SetTile(startGridPosition, startPathTile);
        }
        else
        {
            // Remove the start position from the cell if that cell already contains a start position
            pathTilemap.SetTile(startGridPosition, null);
            startGridPosition = noValue;
        }
    }

    /// <summary>
    /// Set the end position and the end tile on the front tilemap
    /// </summary>
    private void SetEndPosition()
    {
        // Check if the end position has not been populated yet
        if (endGridPosition == noValue)
        {
            // Set the end position to the cell the mouse is hovering over
            endGridPosition = grid.WorldToCell(HelperUtilities.GetMouseWorldPosition());

            // If the position is not within the bounds of the tilemap, do nothing
            if (!IsPositionWithinBounds(endGridPosition))
            {
                endGridPosition = noValue;
                return;
            }

            // Set the end path tile at the mouse position
            pathTilemap.SetTile(endGridPosition, finishPathTile);
        }
        else
        {
            // Remove the end position from the cell if that cell already contains an end position
            pathTilemap.SetTile(endGridPosition, null);
            endGridPosition = noValue;
        }
    }

    /// <summary>
    /// Check if the position is within lower bounds and upper bounds on x and y axes
    /// </summary>
    /// <param name="position"></param>
    /// <returns>True if the position is within bounds, false otherwise</returns>
    private bool IsPositionWithinBounds(Vector3Int position)
    {
        // If position is beyond grid then return false
        if (position.x < instantiatedRoom.room.templateLowerBounds.x || position.x > instantiatedRoom.room.templateUpperBounds.x ||
            position.y < instantiatedRoom.room.templateLowerBounds.y || position.y > instantiatedRoom.room.templateUpperBounds.y)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Clear the path and reset the start and finish positions
    /// </summary>
    private void ClearPath()
    {
        // If the path stack is empty, return
        if (pathStack == null)
        {
            return;
        }

        // Clear each path tile in the grid
        foreach (Vector3 worldPosition in pathStack)
        {
            pathTilemap.SetTile(grid.WorldToCell(worldPosition), null);
        }

        pathStack = null;

        // Clear Start and Finish squares
        endGridPosition = noValue;
        startGridPosition = noValue;
    }

    /// <summary>
    /// Build and display the A* path between the start and finish positions
    /// </summary>
    private void DisplayPath()
    {
        // Check if the starting and ending positons are set
        if (startGridPosition == noValue || endGridPosition == noValue)
        {
            return;
        }

        // Call the actual A* algorithm and build the path
        pathStack = AStar.BuildPath(instantiatedRoom.room, startGridPosition, endGridPosition);

        // If there was no valid path, return
        if (pathStack == null)
        {
            return;
        }

        // For each element in the path stack, set the tile on the grid to the startPathTile
        foreach (Vector3 worldPosition in pathStack)
        {
            pathTilemap.SetTile(grid.WorldToCell(worldPosition), startPathTile);
        }
    }
}
