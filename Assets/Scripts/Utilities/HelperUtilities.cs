using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities
{
    public static Camera mainCamera;

    /// <summary>
    /// Get the mouse world position
    /// </summary>
    /// <returns>World position of mouse</returns>
    public static Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null)
        {
            // Available because we tagged the Main Camera in the Unity scene as "MainCamera"
            mainCamera = Camera.main;
        }

        // Returns and assigns current screen position of the mouse
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Clamp mouse position to screen size (get screen position of mouse to then convert to world position)
        mouseScreenPosition.x = Mathf.Clamp(mouseScreenPosition.x, 0f, Screen.width); // x-component (width)
        mouseScreenPosition.y = Mathf.Clamp(mouseScreenPosition.y, 0f, Screen.height); // y-component (height)

        // Convert screen position to world position
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        // Set world position to 0 (no z-axis as this game is 2D).
        worldPosition.z = 0f;

        return worldPosition;
    }

    /// <summary>
    /// Get the angle in degrees from a direction vector.
    /// Unity calculates angle as 0* --> 180* from quadrant 1 to 2.
    /// Then angle as 0* --> -180* from quadrant 4 to 3.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns>Calculate the angle between the player and the mouse cursor</returns>
    public static float GetAngleFromVector(Vector3 vector)
    {
        // Get angle between player and mouse cursor using arctangent.
        float radians = Mathf.Atan2(vector.y, vector.x);

        // Convert radian angle to degrees
        float degrees = radians * Mathf.Rad2Deg;

        return degrees;
    }

    /// <summary>
    /// Get the direction vector from an angle in degrees
    /// </summary>
    /// <param name="angle"></param>
    /// <returns>Direction vector representing distance from player/weapon to target</returns>
    public static Vector3 GetDirectionVectorFromAngle(float angle)
    {
        Vector3 directionVector = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0f);
        return directionVector;
    }

    /// <summary>
    /// Get AimDirection enum value from the passed in angleDegrees parameter
    /// </summary>
    /// <param name="angleDegrees"></param>
    /// <returns>An AimDirection object based on the input angle</returns>
    public static AimDirection GetAimDirection(float angleDegrees)
    {
        AimDirection aimDirection;

        if (angleDegrees >= 22f && angleDegrees <= 67f)
        {
            aimDirection = AimDirection.UpRight;
        }
        else if (angleDegrees > 67f && angleDegrees <= 112f)
        {
            aimDirection = AimDirection.Up;
        }
        else if (angleDegrees > 112f && angleDegrees <= 158)
        {
            aimDirection = AimDirection.UpLeft;
        }
        else if ((angleDegrees <= 180f && angleDegrees > 158f) || (angleDegrees > -180f && angleDegrees <= -135f))
        {
            aimDirection = AimDirection.Left;
        }
        else if (angleDegrees > -135 && angleDegrees <= -45f)
        {
            aimDirection = AimDirection.Down;
        }
        else if (angleDegrees > -45f && angleDegrees < 22f)
        {
            aimDirection = AimDirection.Right;
        }
        else // Fallback case - will never be executed
        {
            aimDirection = AimDirection.Right;
        }

        return aimDirection;
    }

    public static float LinearToDecibels(int linear)
    {
        float linearScaleRange = 20f;

        // Formula to convert from the linear scale to the logarithmic decibel scale
        return Mathf.Log10((float)linear / linearScaleRange) * 20f;
    }

    /// <summary>
    /// string empty check - returns true is string is empty
    /// </summary>
    /// <param name="thisObject"></param>
    /// <param name="fieldName"></param>
    /// <param name="stringToCheck"></param>
    /// <returns>True if string is empty, false otherwise</returns>
    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck)
    {
        if (stringToCheck == "")
        {
            Debug.Log(fieldName + " is empty and must contain a value in object " + thisObject.name.ToString());
            return true;
        }
        return false;
    }

    /// <summary>
    /// Null value debug check
    /// </summary>
    /// <param name="thisObject"></param>
    /// <param name="fieldName"></param>
    /// <param name="objectToCheck"></param>
    /// <returns>True if objectToCheck is null, false otherwise</returns>
    public static bool ValidateCheckNullValue(Object thisObject, string fieldName, UnityEngine.Object objectToCheck)
    {
        if (objectToCheck == null)
        {
            Debug.Log(fieldName + " is null and must contain a value in object " + thisObject.name.ToString());
            return true;
        }
        return false;
    }

    /// <summary>
    /// list empty or contains null value check - returns true is there is an error
    /// </summary>
    /// <param name="thisObject"></param>
    /// <param name="fieldName"></param>
    /// <param name="enumerableObjectToCheck"></param>
    /// <returns>True if error exists, false otherwise</returns>
    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableObjectToCheck)
    {
        bool error = false;
        int count = 0;

        if (enumerableObjectToCheck == null)
        {
            Debug.Log(fieldName + " is null in object " + thisObject.name.ToString());
            return true;
        }

        foreach (var item in enumerableObjectToCheck)
        {
            if (item == null)
            {
                Debug.Log(fieldName + " has null values in object " + thisObject.name.ToString());
                error = true;
            }
            else
            {
                count++;
            }
        }

        if (count == 0)
        {
            Debug.Log(fieldName + " has no values in object " + thisObject.name.ToString());
            error = true;
        }

        return error;
    }

    /// <summary>
    /// Positive value debug check - if zero is allowed set isZeroAllowed to true
    /// For ints
    /// </summary>
    /// <param name="thisObject"></param>
    /// <param name="fieldName"></param>
    /// <param name="valueToCheck"></param>
    /// <param name="isZeroAllowed"></param>
    /// <returns>True if error</returns>
    public static bool ValidateCheckPositiveValue(Object thisObject, string fieldName, int valueToCheck, bool isZeroAllowed)
    {
        bool error = false;

        if (isZeroAllowed)
        {
            if (valueToCheck < 0)
            {
                Debug.Log(fieldName + " must contain a positive value or zero in object " + thisObject.name.ToString());
                error = true;
            }
        }
        else
        {
            if (valueToCheck <= 0 )
            {
                Debug.Log(fieldName + " must contain a positive value in object " + thisObject.name.ToString());
                error = true;
            }
        }

        return error;
    }

    /// <summary>
    /// Positive value check - if zero is allowed set isZeroAllowed to true.
    /// For floats
    /// </summary>
    /// <param name="thisObject"></param>
    /// <param name="fieldName"></param>
    /// <param name="valueToCheck"></param>
    /// <param name="isZeroAllowed"></param>
    /// <returns>True if error</returns>
    public static bool ValidateCheckPositiveValue(Object thisObject, string fieldName, float valueToCheck, bool isZeroAllowed)
    {
        bool error = false;

        if (isZeroAllowed)
        {
            if (valueToCheck < 0)
            {
                Debug.Log(fieldName + " must contain a positive value or zero in object " + thisObject.name.ToString());
                error = true;
            }
        }
        else
        {
            if (valueToCheck <= 0)
            {
                Debug.Log(fieldName + " must contain a positive value in object " + thisObject.name.ToString());
                error = true;
            }
        }

        return error;
    }

    /// <summary>
    /// Positive range debug check - set isZeroAllowed to true if the min and max range values can both be zero.
    /// </summary>
    /// <param name="thisObject"></param>
    /// <param name="fieldNameMinimum"></param>
    /// <param name="valueToCheckMinimum"></param>
    /// <param name="fieldNameMaximum"></param>
    /// <param name="valueToCheckMaximum"></param>
    /// <param name="isZeroAllowed"></param>
    /// <returns>True if error</returns>
    public static bool ValidateCheckPositiveRange(Object thisObject, string fieldNameMinimum, float valueToCheckMinimum, string fieldNameMaximum, float valueToCheckMaximum, bool isZeroAllowed)
    {
        bool error = false;
        if (valueToCheckMinimum > valueToCheckMaximum)
        {
            Debug.Log(fieldNameMinimum + " must be less than or equal to " + fieldNameMaximum + " in object " + thisObject.name.ToString());
            error = true;
        }

        if (ValidateCheckPositiveValue(thisObject, fieldNameMinimum, valueToCheckMinimum, isZeroAllowed))
        {
            error = true;
        }

        if (ValidateCheckPositiveValue(thisObject, fieldNameMaximum, valueToCheckMaximum, isZeroAllowed))
        {
            error = true;
        }

        return error;
    }

    /// <summary>
    /// Get the nearest spawn position to the player
    /// </summary>
    /// <param name="playerPosition"></param>
    /// <returns>Position of nearest spawn point to player</returns>
    public static Vector3 GetSpawnPositionNearestToPlayer(Vector3 playerPosition)
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();

        // Need a grid component to convert a tilemap position to a world position
        Grid grid = currentRoom.instantiatedRoom.grid;

        // Once a new closer spawn position is found, overwrite this variable with that value
        Vector3 nearestSpawnPosition = new Vector3(10000f, 10000f, 0f);

        // Loop through room spawn positions
        foreach (Vector2Int spawnPositionGrid in currentRoom.spawnPositionArray)
        {
            // Convert the spawn grid positions to world positions
            Vector3 spawnPositionWorld = grid.CellToWorld((Vector3Int)spawnPositionGrid);

            // If the new spawn position is closer than the current spawn position, overwrite the nearestSpawnPosition variable
            if (Vector3.Distance(spawnPositionWorld, playerPosition) < Vector3.Distance(nearestSpawnPosition, playerPosition))
            {
                // Set new nearest spawn position
                nearestSpawnPosition = spawnPositionWorld;
            }
        }

        return nearestSpawnPosition;
    }
}
