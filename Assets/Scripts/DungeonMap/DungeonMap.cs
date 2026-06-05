using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMap : SingletonMonobehavior<DungeonMap>
{
    #region Header GAMEOBJECT REFERENCES
    [Header("GAMEOBJECT REFERENCES")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the MinimapUI gameobject (to be enabled/disabled)")]
    #endregion
    [SerializeField] private GameObject minimapUI;
    
    private Camera dungeonMapCamera;
    private Camera mainCamera;

    private void Start()
    {
        // Cache main camera
        mainCamera = Camera.main;

        // Get player transform (for cinemachine virtual camera to follow)
        Transform playerTransform = GameManager.Instance.GetPlayer().transform;

        // Populate player as cinemachine camera follow target
        CinemachineVirtualCamera cinemachineVirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.Follow = playerTransform;

        // Get dungeon map camera
        dungeonMapCamera = GetComponentInChildren<Camera>();

        // Don't show dungeon map until the player wants to see it
        dungeonMapCamera.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Check if the user clicks on the map when the dungeon overview map is displayed
        if (Input.GetMouseButtonDown(0) && GameManager.Instance.gameState == GameState.dungeonOverviewMap)
        {
            // Get the room that was clicked on
            GetRoomClicked();
        }
    }

    /// <summary>
    /// Figure out which room was clicked on (if any)
    /// </summary>
    private void GetRoomClicked()
    {
        // Get the world position of the spot that was clicked on the map
        Vector3 worldPosition = dungeonMapCamera.ScreenToWorldPoint(Input.mousePosition);

        // Reset z-coordinate
        worldPosition = new Vector3(worldPosition.x, worldPosition.y, 0f);

        // Get a list of all colliders that fall within one unity-unit from the location of the mouse cursor
        Collider2D[] collider2DArray = Physics2D.OverlapCircleAll(new Vector2(worldPosition.x, worldPosition.y), 1f);

        // Iterate through each of the overlapped colliders
        foreach (Collider2D collider2D in collider2DArray)
        {
            // Check if the current collider being checked has an instantiated room component attached to it
            if (collider2D.GetComponent<InstantiatedRoom>() != null)
            {
                // Get the instantiated room that was clicked on
                InstantiatedRoom instantiatedRoom = collider2D.GetComponent<InstantiatedRoom>();

                // Check if the room is clear of enemies and has been previously visited
                if (instantiatedRoom.room.isPreviouslyVisited && instantiatedRoom.room.isClearedOfEnemies)
                {
                    // Move the player to the room
                    StartCoroutine(MovePlayerToRoom(worldPosition, instantiatedRoom.room));
                }
            }
        }
    }

    /// <summary>
    /// Move the player to the specified location (which should be a room)
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <param name="room"></param>
    /// <returns>Coroutine</returns>
    private IEnumerator MovePlayerToRoom(Vector3 worldPosition, Room room)
    {
        // Make sure everything that should happen when the player enters a different room does happen
        StaticEventHandler.CallRoomChangedEvent(room);

        // Fade out screen to black immediately
        yield return StartCoroutine(GameManager.Instance.Fade(0f, 1f, 0f, Color.black));

        // Clear dungeon overview map
        ClearDungeonOverviewMap();

        // Disable play during fade
        GameManager.Instance.GetPlayer().playerControl.DisablePlayer();

        // Figure out nearest spawn position to place the player
        Vector3 spawnPosition = HelperUtilities.GetSpawnPositionNearestToPlayer(worldPosition);

        // Set the player's position to the nearest spawn position
        GameManager.Instance.GetPlayer().transform.position = spawnPosition;
        
        // Fade the screen back in
        yield return StartCoroutine(GameManager.Instance.Fade(1f, 0f, 1f, Color.black));
        
        // Reenable the player
        GameManager.Instance.GetPlayer().playerControl.EnablePlayer();
    }

    /// <summary>
    /// Display dungeon overview map UI
    /// </summary>
    public void DisplayDungeonOverviewmap()
    {
        // Save and set game state
        GameManager.Instance.previousGameState = GameManager.Instance.gameState;
        GameManager.Instance.gameState = GameState.dungeonOverviewMap;

        // Disable player
        GameManager.Instance.GetPlayer().playerControl.DisablePlayer();

        // Disable main camera and enable dungeon overview camera
        mainCamera.gameObject.SetActive(false);
        dungeonMapCamera.gameObject.SetActive(true);

        // Ensure all rooms are active so they can be displayed
        ActivateRoomsForDisplay();

        // Disable minimap
        minimapUI.SetActive(false);
    }

    /// <summary>
    /// Clear dungeon overview map UI
    /// </summary>
    public void ClearDungeonOverviewMap()
    {
        // Save and set game state
        GameManager.Instance.gameState = GameManager.Instance.previousGameState;
        GameManager.Instance.previousGameState = GameState.dungeonOverviewMap;

        // Re-enable the player
        GameManager.Instance.GetPlayer().playerControl.EnablePlayer();

        // Disable dungeon map camera and enable main camera
        mainCamera.gameObject.SetActive(true);
        dungeonMapCamera.gameObject.SetActive(false);

        // Enable minimap
        minimapUI.SetActive(true);
    }

    /// <summary>
    /// Ensure all rooms are active so they can be displayed
    /// </summary>
    private void ActivateRoomsForDisplay()
    {
        // Iterate through each room in the current dungeon level
        foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            // Get the room for the current iteration
            Room room = keyValuePair.Value;

            // Activate the room
            room.instantiatedRoom.gameObject.SetActive(true);
        }
    }
}
