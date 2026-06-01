using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]

public class MoveItem : MonoBehaviour
{
    #region Header SOUND EFFECT
    [Header("SOUND EFFECT")]
    #endregion
    #region Tooltip
    [Tooltip("The sound effect when this item is moved")]
    #endregion
    [SerializeField] private SoundEffectSO moveSoundEffect;

    [HideInInspector] public BoxCollider2D boxCollider2D;
    private Rigidbody2D rigidbody2D;
    private InstantiatedRoom instantiatedRoom;
    private Vector3 previousPosition;

    private void Awake()
    {
        // Load components
        boxCollider2D = GetComponent<BoxCollider2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        instantiatedRoom = GetComponentInParent<InstantiatedRoom>();

        // Add current item to item obstacles list
        instantiatedRoom.moveableItemsList.Add(this);
    }

    /// <summary>
    /// Update the obstacle position when something comes into contact
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateObstacles();
    }

    /// <summary>
    /// Update obstacle positions
    /// </summary>
    private void UpdateObstacles()
    {
        // Make sure the item stays within the room
        ConfineItemToRoomBounds();

        // Update moveable items in obstacles list
        instantiatedRoom.UpdateMoveableObstacles();

        // Capture new position post collision
        previousPosition = transform.position;

        // Check if should play moving item sound effect
        if (Mathf.Abs(rigidbody2D.velocity.x) > 0.001f || Mathf.Abs(rigidbody2D.velocity.y) > 0.001f)
        {
            // Check if sound effect exists and make sure sound is only played every 10 frames
            if (moveSoundEffect != null && Time.frameCount % 10 == 0)
            {
                // Play the sound effect
                SoundEffectManager.Instance.PlaySoundEffect(moveSoundEffect);
            }
        }
    }

    /// <summary>
    /// Confine the item to stay within the room bounds
    /// </summary>
    private void ConfineItemToRoomBounds()
    {
        Bounds itemBounds = boxCollider2D.bounds; // Bounds for the item
        Bounds roomBounds = instantiatedRoom.roomColliderBounds; // Bounds for the room

        // Check if the item is being pushed beyond the room bounds
        if (itemBounds.min.x <= roomBounds.min.x || 
            itemBounds.max.x >= roomBounds.max.x ||
            itemBounds.min.y <= roomBounds.min.y ||
            itemBounds.max.y >= roomBounds.max.y)
        {
            // Set the current position to the last valid position
            transform.position = previousPosition;
        }
    }
}
