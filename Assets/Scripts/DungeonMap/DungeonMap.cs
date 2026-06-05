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
