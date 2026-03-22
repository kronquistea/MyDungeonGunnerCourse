using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[DisallowMultipleComponent]

public class MovementToPositionEvent : MonoBehaviour
{
    public event Action<MovementToPositionEvent, MovementToPositionArgs> OnMovementToPosition;

    public void CallMovementToPositionEvent(Vector3 movePosition, Vector3 currentPosition, float moveSpeed, Vector2 moveDirection, bool isRollling = false)
    {
        OnMovementToPosition?.Invoke(this, new MovementToPositionArgs() { movePosition = movePosition, currentPosition = currentPosition, moveSpeed = moveSpeed, moveDirection = moveDirection, isRolling = isRollling });
    }
}

public class MovementToPositionArgs : EventArgs
{
    public Vector3 movePosition;
    public Vector3 currentPosition;
    public float moveSpeed;
    public Vector2 moveDirection;
    public bool isRolling;
}
