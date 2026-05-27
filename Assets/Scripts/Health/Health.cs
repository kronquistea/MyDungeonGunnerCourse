using System.Collections;
using UnityEngine;

[RequireComponent(typeof(HealthEvent))]

[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    private int startingHealth;
    private int currentHealth;
    private HealthEvent healthEvent;
    private Player player;
    private Enemy enemy;

    [HideInInspector] public bool isDamageable = true;

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
        }

        if (isDamageable && isRolling)
        {
            Debug.Log("Dodged bullet by rolling");
        }
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
