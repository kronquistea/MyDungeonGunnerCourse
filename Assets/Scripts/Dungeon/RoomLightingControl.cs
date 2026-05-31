using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(InstantiatedRoom))]
[DisallowMultipleComponent]

public class RoomLightingControl : MonoBehaviour
{
    private InstantiatedRoom instantiatedRoom;

    private void Awake()
    {
        // Load components
        instantiatedRoom = GetComponent<InstantiatedRoom>();
    }

    private void OnEnable()
    {
        // Subscribe to room changed event
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        // Unsuscribe from room changed event
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    /// <summary>
    /// Handle room changed event
    /// </summary>
    /// <param name="roomChangedEventArgs"></param>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        // If this is the room entered and the room isn't already lit, then fade in the room lighting
        if (roomChangedEventArgs.room == instantiatedRoom.room && !instantiatedRoom.room.isLit)
        {
            // Fade in room
            FadeInRoomLighting();

            // Ensure room environment decoration game objects are activated
            instantiatedRoom.ActivateEnvironmentGameObjects();

            // Fade in environment decoration gameobjects lighting
            FadeInEnvironmentLighting();

            // Fade in the room doors lighting
            FadeInDoors();

            instantiatedRoom.room.isLit = true;
        }
    }

    /// <summary>
    /// Call coroutine to fade in room lighting
    /// </summary>
    private void FadeInRoomLighting()
    {
        // Fade in the lighting for the room tilemaps
        StartCoroutine(FadeInRoomLightingRoutine(instantiatedRoom));
    }

    /// <summary>
    /// Fade in the room lighting coroutine
    /// </summary>
    /// <param name="instantiatedRoom"></param>
    /// <returns></returns>
    private IEnumerator FadeInRoomLightingRoutine(InstantiatedRoom instantiatedRoom)
    {
        // Create new material to fade in
        Material material = new Material(GameResources.Instance.variableLitShader);

        instantiatedRoom.groundTilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.decoration1Tilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.decoration2Tilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.frontTilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.minimapTilemap.GetComponent<TilemapRenderer>().material = material;

        for (float i = 0.05f; i <= 1f; i += Time.deltaTime / Settings.fadeInTime)
        {
            material.SetFloat("Alpha_Slider", i);
            yield return null;
        }

        // Set material back to lit material
        instantiatedRoom.groundTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.decoration1Tilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.decoration2Tilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.frontTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.minimapTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
    }

    /// <summary>
    /// Fade in the environmental decoration game objects
    /// </summary>
    private void FadeInEnvironmentLighting()
    {
        // Create new material to fade in
        Material material = new Material(GameResources.Instance.variableLitShader);

        // Get all environment components in room
        Environment[] environmentComponents = GetComponentsInChildren<Environment>();

        // Loop through each environment component object
        foreach (Environment environmentComponent in environmentComponents)
        {
            // Check if the environment component has a sprite renderer
            if (environmentComponent.spriteRenderer != null)
            {
                // Set the material for the sprite renderer to the newly created material above
                environmentComponent.spriteRenderer.material = material;

                //Debug.Log("environmentComponent.gameObject.transform.childCount: " + environmentComponent.gameObject.transform.childCount);

                if (environmentComponent.gameObject.transform.childCount > 0)
                {
                    GameObject flameGameObject = environmentComponent.gameObject.transform.GetChild(0).gameObject;
                    SpriteRenderer flameSpriteRenderer = flameGameObject.GetComponent<SpriteRenderer>();
                    int randomFlameMaterialIndex = Random.Range(0, 2);
                    switch (randomFlameMaterialIndex)
                    {
                        case 0:
                            flameSpriteRenderer.material = new Material(GameResources.Instance.flameShaderMaterialZero);
                            break;
                        case 1:
                            flameSpriteRenderer.material = new Material(GameResources.Instance.flameShaderMaterialOne);
                            break;
                        case 2:
                            flameSpriteRenderer.material = new Material(GameResources.Instance.flameShaderMaterialTwo);
                            break;
                    }
                }
            }
        }

        // Start a coroutine to fade in the environment lighting
        StartCoroutine(FadeInEnvironmentLightingRoutine(material, environmentComponents));
    }

    /// <summary>
    /// Fade in the environmental decoration game objects coroutine
    /// </summary>
    /// <param name="material"></param>
    /// <param name="environmentComponents"></param>
    /// <returns>Coroutine</returns>
    private IEnumerator FadeInEnvironmentLightingRoutine(Material material, Environment[] environmentComponents)
    {
        // Fade in the lighting for the objects
        for (float i = 0.05f; i <= 1f; i += Time.deltaTime / Settings.fadeInTime)
        {
            material.SetFloat("Alpha_Slider", i);
            yield return null;
        }

        // Set environment component material back to lit material
        foreach (Environment environmentComponent in environmentComponents)
        {
            // Check if environment component has a sprite renderer
            if (environmentComponent.spriteRenderer != null)
            {
                // Set the sprite renderer material back to litMaterial
                environmentComponent.spriteRenderer.material = GameResources.Instance.litMaterial;
            }
        }
    }

    /// <summary>
    /// Fade in doors
    /// </summary>
    private void FadeInDoors()
    {
        Door[] doorArray = GetComponentsInChildren<Door>();

        foreach (Door door in doorArray)
        {
            DoorLightingControl doorLightingControl = door.GetComponentInChildren<DoorLightingControl>();

            doorLightingControl.FadeInDoor(door);
        }
    }
}
