using System.Collections;
using UnityEngine;

[RequireComponent(typeof(HealthEvent))]

[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    #region Header REFERENCES
    [Space(10)]
    [Header("REFERENCES")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the HealthBar component on the HealthBar gameobject")]
    #endregion
    [SerializeField] private HealthBar healthBar;

    private int startingHealth;
    private int currentHealth;
    private HealthEvent healthEvent;
    private Player player;
    private Coroutine immunityCoroutine;
    private bool isImmuneAfterHit = false;
    private float immunityTime = 0f;
    private SpriteRenderer spriteRenderer = null;
    private const float spriteFlashInterval = 0.2f;
    private WaitForSeconds WaitForSecondsSpriteFlashInterval = new WaitForSeconds(spriteFlashInterval);

    [HideInInspector] public bool isDamageable = true;
    [HideInInspector] public Enemy enemy;

    private void Awake()
    {
        // Load components
        healthEvent = GetComponent<HealthEvent>();
    }

    private void Start()
    {
        // Trigger a health event for UI update
        CallHealthEvent(0);

        // Attempt to load player / enemy components
        player = GetComponent<Player>();
        enemy = GetComponent<Enemy>();

        // Get player hit immunity details
        if (player != null)
        {
            if (player.playerDetails.isImmuneAfterHit)
            {
                isImmuneAfterHit = true;
                immunityTime = player.playerDetails.hitImmunityTime;
                spriteRenderer = player.spriteRenderer;
            }
        }
        // Get enemy hit immunity details
        else if (enemy != null)
        {
            if (enemy.enemyDetails.isImmuneAfterHit)
            {
                isImmuneAfterHit = true;
                immunityTime = enemy.enemyDetails.hitImmunityTime;
                spriteRenderer = enemy.spriteRendererArray[0];
            }
        }

        // Check if health bar should be enabled
        if (enemy != null && enemy.enemyDetails.isHealthBarDisplayed && healthBar != null)
        {
            healthBar.EnableHealthBar();
        }
        // Check if health bar should be disabled
        else if (healthBar != null)
        {
            healthBar.DisableHealthBar();
        }
    }

    /// <summary>
    /// Public method to be called when damage is taken
    /// </summary>
    /// <param name="damageAmount"></param>
    public void TakeDamage(int damageAmount)
    {
        bool isRolling = false;
        if (player != null)
        {
            isRolling = player.playerControl.isPlayerRolling;
        }

        // Only apply damage to enemies and non-rolling players
        if (isDamageable && !isRolling)
        {
            currentHealth -= damageAmount;
            CallHealthEvent(damageAmount);

            PostHitImmunity();

            // Check if health bar exists and should therefore be updated
            if (healthBar != null)
            {
                // Set health bar value based on current health as a percentage of starting health
                healthBar.SetHealthBarValue((float)currentHealth / startingHealth);
            }
        }
    }

    /// <summary>
    /// Indicate a hit and give post hit immunity
    /// </summary>
    private void PostHitImmunity()
    {
        // Check if gameObject is active - if not then return
        if (gameObject.activeSelf == false)
        {
            return;
        }

        if (isImmuneAfterHit)
        {
            if (immunityCoroutine != null)
            {
                StopCoroutine(immunityCoroutine);
            }

            // Flash red on the character and give immunity
            immunityCoroutine = StartCoroutine(PostHitImmunityRoutine(immunityTime, spriteRenderer));
        }
    }

    /// <summary>
    /// Coroutine to indicate a hit and give some post hit immunity
    /// </summary>
    /// <param name="immunityTime"></param>
    /// <param name="spriteRenderer"></param>
    /// <returns>Coroutine</returns>
    private IEnumerator PostHitImmunityRoutine(float immunityTime, SpriteRenderer spriteRenderer)
    {
        // ex. 5 seconds of immunity, flash interval of 0.2 seconds, so 5/0.2 = 25, 25/2 = 12.5, round to int = 12 iterations
        int iterations = Mathf.RoundToInt(immunityTime / spriteFlashInterval / 2f);

        // Grant post-hit immunity
        isDamageable = false;

        while (iterations > 0)
        {
            spriteRenderer.color = Color.red;

            yield return WaitForSecondsSpriteFlashInterval;

            spriteRenderer.color = Color.white;

            yield return WaitForSecondsSpriteFlashInterval;

            iterations--;

            yield return null; // Wait until next frame
        }

        isDamageable = true;
    }

    /// <summary>
    /// Call health changed event to set new health related event parameters
    /// </summary>
    /// <param name="damageAmount"></param>
    private void CallHealthEvent(int damageAmount)
    {
        // Trigger health event
        healthEvent.CallHealthChangedEvent(((float)currentHealth / (float)startingHealth), currentHealth, damageAmount);
    }

    /// <summary>
    /// Set starting health
    /// </summary>
    /// <param name="startingHealth"></param>
    public void SetStartingHealth(int startingHealth)
    {
        this.startingHealth = startingHealth;
        currentHealth = startingHealth;
    }

    /// <summary>
    /// Get starting health
    /// </summary>
    /// <returns>Starting health (int)</returns>
    public int GetStartingHealth()
    {
        return startingHealth;
    }
}
