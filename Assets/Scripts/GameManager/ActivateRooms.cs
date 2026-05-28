using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]

public class ActivateRooms : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate with minimap camera")]
    #endregion
    [SerializeField] private Camera miniMapCamera;

    private void Start()
    {
        // Call EnableRooms after 0.5 seconds, then repeatedly call it every 0.75 seconds after the initial delay
        InvokeRepeating("EnableRooms", 0.5f, 0.75f);
    }

    /// <summary>
    /// Enable/Disable rooms based on viewport (minimap camera in this case) visibility to them
    /// </summary>
    private void EnableRooms()
    {
        // Iterate through dungeon rooms in the current level
        foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;

            HelperUtilities.CameraWorldPositionBounds(out Vector2Int miniMapCameraWorldPositionLowerBounds, out Vector2Int miniMapCameraWorldPositionUpperBounds, miniMapCamera);

            // Check if the room is within the viewport of the camera (minimap camera)
            if ((room.lowerBounds.x <= miniMapCameraWorldPositionUpperBounds.x && room.lowerBounds.y <= miniMapCameraWorldPositionUpperBounds.y) &&
                (room.upperBounds.x >= miniMapCameraWorldPositionLowerBounds.x && room.upperBounds.y >= miniMapCameraWorldPositionLowerBounds.y))
            {
                // Activate the room
                room.instantiatedRoom.gameObject.SetActive(true);
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
