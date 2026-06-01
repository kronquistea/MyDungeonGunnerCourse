using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]

public class Table : MonoBehaviour, IUseable
{
    #region Tooltip
    [Tooltip("The mass of the table to control the speed that it moves when pushed")]
    #endregion
    [SerializeField] private float itemMass;

    private BoxCollider2D boxCollider2D;
    private Animator animator;
    private Rigidbody2D rigidBody2D;
    private bool itemUsed = false;

    private void Awake()
    {
        // Load components
        boxCollider2D = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        rigidBody2D = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 
    /// </summary>
    public void UseItem()
    {
        // Check to make sure the item has not yet been used (the table has not yet been flipped)
        if (!itemUsed)
        {
            // Get the bounds for the table
            Bounds bounds = boxCollider2D.bounds;

            // Get the closest point from the table bounds to the position of the player
            Vector3 closestPointToPlayer = bounds.ClosestPoint(GameManager.Instance.GetPlayer().GetPlayerPosition());

            // Check if the player is to the right of the table
            if (closestPointToPlayer.x == bounds.max.x)
            {
                // Flip the table to the left
                animator.SetBool(Settings.flipLeft, true);
            }
            // Else check if the player is to the left of the table
            else if (closestPointToPlayer.x == bounds.min.x)
            {
                // Flip the table to the right
                animator.SetBool(Settings.flipRight, true);
            }
            // Else check if the player is above the table
            else if (closestPointToPlayer.y == bounds.max.y)
            {
                // Flip the table down
                animator.SetBool(Settings.flipDown, true);
            }
            // Else the player must be below the table
            else
            {
                // Flip the table up
                animator.SetBool(Settings.flipUp, true);
            }

            // Enable the item to collide with bullets and what not
            gameObject.layer = LayerMask.NameToLayer("Environment");

            // Enable the mass to the specified item mass (to be able to be pushed around)
            rigidBody2D.mass = itemMass;

            // Play table flip sound effect
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.tableFlip);
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(itemMass), itemMass, false);
    }
#endif
    #endregion
}
