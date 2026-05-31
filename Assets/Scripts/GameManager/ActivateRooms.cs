using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]

public class ActivateRooms : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate with minimap camera")]
    #endregion
    [SerializeField] private Camera miniMapCamera;

    private Camera cameraMain;

    private void Start()
    {
        // Cache main camera reference
        cameraMain = Camera.main;

        // Call EnableRooms after 0.5 seconds, then repeatedly call it every 0.75 seconds after the initial delay
        InvokeRepeating("EnableRooms", 0.5f, 0.75f);
    }

    /// <summary>
    /// Enable/Disable rooms and related items (decoration objects) based on minimap camera viewport and main camera viewport respectively.
    /// </summary>
    private void EnableRooms()
    {
        HelperUtilities.CameraWorldPositionBounds(out Vector2Int miniMapCameraWorldPositionLowerBounds, out Vector2Int miniMapCameraWorldPositionUpperBounds, miniMapCamera);

        HelperUtilities.CameraWorldPositionBounds(out Vector2Int mainCameraWorldPositionLowerBounds, out Vector2Int mainCameraWorldPositionUpperBounds, cameraMain);

        // Iterate through dungeon rooms in the current level
        foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;

            // Check if the room is within the viewport of the camera (minimap camera)
            if ((room.lowerBounds.x <= miniMapCameraWorldPositionUpperBounds.x && room.lowerBounds.y <= miniMapCameraWorldPositionUpperBounds.y) &&
                (room.upperBounds.x >= miniMapCameraWorldPositionLowerBounds.x && room.upperBounds.y >= miniMapCameraWorldPositionLowerBounds.y))
            {
                // Activate the room
                room.instantiatedRoom.gameObject.SetActive(true);

                // Check if room is within main camera viewport (for loading items)
                if ((room.lowerBounds.x <= mainCameraWorldPositionUpperBounds.x && room.lowerBounds.y <= mainCameraWorldPositionUpperBounds.y) &&
                    (room.upperBounds.x >= mainCameraWorldPositionLowerBounds.x && room.upperBounds.y >= mainCameraWorldPositionLowerBounds.y))
                {
                    // If the main camera is within viewing of the room, load the items
                    room.instantiatedRoom.ActivateEnvironmentGameObjects();
                }
                else
                {
                    // Otherwise unload/deactivate the items
                    room.instantiatedRoom.DeactivateEnvironmentGameObjects();
                }
            }
            // Room is not within the viewport of the camera (minimap camera)
            else
            {
                // Disable the room
                room.instantiatedRoom.gameObject.SetActive(false);
            }
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(miniMapCamera), miniMapCamera);
    }
#endif
    #endregion
}
