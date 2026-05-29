using UnityEngine;

public class HealthBar : MonoBehaviour
{
    #region Header GAMEOBJECT REFERENCES
    [Space(10)]
    [Header("GAMEOBJECT REFERENCES")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the child Bar gameobject")]
    #endregion
    [SerializeField] private GameObject healthBar;

    /// <summary>
    /// Set health bar as active
    /// </summary>
    public void EnableHealthBar()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Set health bar as inactive
    /// </summary>
    public void DisableHealthBar()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Set health bar value based on health percent
    /// </summary>
    /// <param name="healthPercent">Float between 0 and 1</param>
    public void SetHealthBarValue(float healthPercent)
    {
        healthBar.transform.localScale = new Vector3(healthPercent, 1f, 1f);
    }
}
