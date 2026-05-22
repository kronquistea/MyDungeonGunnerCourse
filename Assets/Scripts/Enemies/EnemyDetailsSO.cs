using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDetails_", menuName = "Scriptable Objects/Enemy/EnemyDetails")]

public class EnemyDetailsSO : ScriptableObject
{
    #region Header BASE ENEMY DETAILS
    [Space(10)]
    [Header("BASE ENEMY DETAILS")]
    #endregion
    #region Tooltip
    [Tooltip("The name of the enemy")]
    #endregion
    public string enemyName;

    #region Tooltip
    [Tooltip("The prefab of the enemy")]
    #endregion
    public GameObject enemyPrefab;

    #region Tooltip
    [Tooltip("Distance to the player before enemy starts chasing")]
    #endregion
    public float chaseDistance = 50f;

    #region Header ENEMY MATERIAL
    [Space(10)]
    [Header("ENEMY MATERIAL")]
    #endregion
    #region Tooltip
    [Tooltip("This is the standard lit shader material for the enemy (used after the enemy materializes)")]
    #endregion
    public Material enemyStandardMaterial;

    #region Header ENEMY MATERIALIZE SETTINGS
    [Space(10)]
    [Header("ENEMY MATERIALIZE SETTINGS")]
    #endregion
    #region Tooltip
    [Tooltip("The time in seconds that it takes the enemy to materialize")]
    #endregion
    public float enemyMaterializeTime;

    #region Tooltip
    [Tooltip("The shader to be used when the enemy materializes")]
    #endregion
    public Shader enemyMaterializeShader;

    [ColorUsage(true, true)]
    #region Tooltip
    [Tooltip("The color to use when the enemy materializes. This is an HDR color so intensity can be set to cause glowing/bloom")]
    #endregion
    public Color enemyMaterializeColor;

    #region Header ENEMY WEAPON SETTINGS
    [Space(10)]
    [Header("ENEMY WEAPON SETTINGS")]
    #endregion
    #region Tooltip
    [Tooltip("The weapon for the enemy - non if the enemy doesn't have a weapon")]
    #endregion
    public WeaponDetailsSO enemyWeapon;

    #region Tooltip
    [Tooltip("The minimum time delay interval in second between bursts of enemy shooting. This value should be greater than 0.")]
    #endregion
    public float firingIntervalMin = 0.1f;

    #region Tooltip
    [Tooltip("The maximum time delay interval in second between bursts of enemy shooting.")]
    #endregion
    public float firingIntervalMax = 1f;

    #region Tooltip
    [Tooltip("The minimum firing duration that the enemy shoots for during a firing burst. This value should be greater than 0.")]
    #endregion
    public float firingDurationMin = 1f;

    #region Tooltip
    [Tooltip("The maximum firing duration that the enemy shoots for during a firing burst.")]
    #endregion
    public float firingDurationMax = 1f;

    #region Tooltip
    [Tooltip("Select this if line of sight is required of the player before the enemy fires.")]
    #endregion
    public bool firingLineOfSightRequired;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        // ENEMY DETAILS
        HelperUtilities.ValidateCheckEmptyString(this, nameof(enemyName), enemyName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyPrefab), enemyPrefab);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(chaseDistance), chaseDistance, false);

        // ENEMY MATERIAL
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyStandardMaterial), enemyStandardMaterial);

        // ENEMY MATERIALIZE SETTINGS
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(enemyMaterializeTime), enemyMaterializeTime, true);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyMaterializeShader), enemyMaterializeShader);

        // ENEMY WEAPON SETTINGS
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(firingIntervalMin), firingIntervalMin, nameof(firingIntervalMax), firingIntervalMax, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(firingDurationMin), firingDurationMin, nameof(firingDurationMax), firingDurationMax, false);
    }
#endif
    #endregion
}
