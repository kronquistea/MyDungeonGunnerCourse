using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementDetails_", menuName = "Scriptable Objects/Movement/MovementDetails")]

public class MovementDetailsSO : ScriptableObject
{
    #region Header MOVEMENT DETAILS
    [Space(10)]
    [Header("MOVEMENT DETAILS")]
    #endregion

    #region Tooltip
    [Tooltip("The minimum move speed. THe GetMoveSpeed method calculates a random value between the minimum and maximim")]
    #endregion
    public float minMoveSpeed = 8f;
}
