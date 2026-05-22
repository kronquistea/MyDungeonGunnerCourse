using UnityEngine;

[RequireComponent(typeof(Enemy))]

[DisallowMultipleComponent]

public class EnemyWeaponAI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Select the layers that the enemy bullets will hit")]
    #endregion
    [SerializeField] private LayerMask layerMask;

    #region Tooltip
    [Tooltip("Populate this with the WeaponShootPosition child gameobject transform")]
    #endregion
    [SerializeField] private Transform weaponShootPosition;

    private Enemy enemy;
    private EnemyDetailsSO enemyDetails;
    private float firingIntervalTimer;
    private float firingDurationTimer;

    private void Awake()
    {
        // Load components
        enemy = GetComponent<Enemy>();
    }

    private void Start()
    {
        enemyDetails = enemy.enemyDetails;

        firingIntervalTimer = WeaponShootInterval();
        firingDurationTimer = WeaponShootDuration();
    }

    private void Update()
    {
        // Update shoot interval timer
        firingIntervalTimer -= Time.deltaTime;

        // Check to see if the enemy should shoot again
        if (firingIntervalTimer < 0f)
        {
            // Check if the enemy should continue shooting
            if (firingDurationTimer >= 0f)
            {
                // Update shoot duration timer
                firingDurationTimer -= Time.deltaTime;

                // Shoot enemy weapon
                FireWeapon();
            }
            else
            {
                // Reset timers
                firingIntervalTimer = WeaponShootInterval();
                firingDurationTimer = WeaponShootDuration();
            }
        }
    }

    /// <summary>
    /// Calculate a random weapon shoot interval between the min and max values
    /// </summary>
    /// <returns>Float - random time (in seconds) the enemy should wait before shooting</returns>
    private float WeaponShootInterval()
    {
        return Random.Range(enemyDetails.firingIntervalMin, enemyDetails.firingIntervalMax);
    }

    /// <summary>
    /// Calculate a random weapon shoot duration between the min and max values
    /// </summary>
    /// <returns>Float - random duration (in seconds) for enemy to shoot for</returns>
    private float WeaponShootDuration()
    {
        return Random.Range(enemyDetails.firingDurationMin, enemyDetails.firingDurationMax);
    }

    /// <summary>
    /// Fire the weapon
    /// </summary>
    private void FireWeapon()
    {
        // Get distance to player (from enemy)
        Vector3 playerDirectionVector = GameManager.Instance.GetPlayer().GetPlayerPosition() - transform.position;

        // Calculate direction vector of player from weapon shoot position (of enemy)
        Vector3 weaponDirection = GameManager.Instance.GetPlayer().GetPlayerPosition() - weaponShootPosition.position;

        // Get angle from weapon (enemy) to player
        float weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        // Get angle from enemy to player
        float enemyAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirectionVector);

        // Set enemy aim direction
        AimDirection enemyAimDirection = HelperUtilities.GetAimDirection(enemyAngleDegrees);

        // Trigger weapon aim event
        enemy.aimWeaponEvent.CallAimWeaponEvent(enemyAimDirection, enemyAngleDegrees, weaponAngleDegrees, weaponDirection);

        // Only fire if the enemy has a weapon
        if (enemyDetails.enemyWeapon != null)
        {
            // Get range for the ammo
            float enemyAmmoRange = enemyDetails.enemyWeapon.weaponCurrentAmmo.ammoRange;

            // Check if the player is in range of the ammo for the enemy weapon
            if (playerDirectionVector.magnitude <= enemyAmmoRange)
            {
                // Check if the enemy requires line of sight to the player before firing
                if (enemyDetails.firingLineOfSightRequired && !IsPlayerInLineOfSight(weaponDirection, enemyAmmoRange))
                {
                    return;
                }

                // Trigger fire weapon event
                enemy.fireWeaponEvent.CallFireWeaponEvent(true, true, enemyAimDirection, enemyAngleDegrees, weaponAngleDegrees, weaponDirection);
            }
        }
    }

    /// <summary>
    /// Check if the player is in line of sight with the enemy by casting a 2D ray from the enemy to the player
    /// </summary>
    /// <param name="weaponDirection"></param>
    /// <param name="enemyAmmoRange"></param>
    /// <returns>True if the enemy has line of sight (via raycase), false otherwise</returns>
    private bool IsPlayerInLineOfSight(Vector3 weaponDirection, float enemyAmmoRange)
    {
        // Get a raycasehit2D object which contains information about the item that the ray hit
        RaycastHit2D raycastHit2D = Physics2D.Raycast(weaponShootPosition.position, (Vector2)weaponDirection, enemyAmmoRange, layerMask);

        // If the ray actually hit something and the thing it hit has the tag of playerTag, then the enemy has line of sight
        if (raycastHit2D && raycastHit2D.transform.CompareTag(Settings.playerTag))
        {
            return true;
        }

        // Enemy does not have line of sight
        return false;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponShootPosition), weaponShootPosition);
    }
#endif
    #endregion
}
