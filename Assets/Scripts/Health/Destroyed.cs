using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DestroyedEvent))]

[DisallowMultipleComponent]

public class Destroyed : MonoBehaviour
{
    private DestroyedEvent destroyedEvent;

    private void Awake()
    {
        // Load components
        destroyedEvent = GetComponent<DestroyedEvent>();
    }

    private void OnEnable()
    {
        // Subscribe to destroyed event
        destroyedEvent.OnDestroyed += DestroyedEvent_OnDestroyed;
    }

    private void OnDisable()
    {
        // Unsubscribe from destroyed event
        destroyedEvent.OnDestroyed -= DestroyedEvent_OnDestroyed;
    }

    private void DestroyedEvent_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        // Check if the player died (as opposed to an enemy/boss)
        if (destroyedEventArgs.playerDied)
        {
            // Disable the player game object to stop enemy pathfinding
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
