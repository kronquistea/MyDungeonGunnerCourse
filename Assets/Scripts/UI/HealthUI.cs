using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]

public class HealthUI : MonoBehaviour
{
    private List<GameObject> healthHeartsList = new List<GameObject>();

    private void OnEnable()
    {
        // Subscribe to health changed event
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from health changed event
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        SetHealthBar(healthEventArgs);
    }

    /// <summary>
    /// Clear the health bar of heart icons
    /// </summary>
    private void ClearHealthBar()
    {
        // Destroy each heart icon remaining the list of hearts
        foreach (GameObject heartIcon in healthHeartsList)
        {
            Destroy(heartIcon);
        }

        // Clear the list of health hearts
        healthHeartsList.Clear();
    }

    /// <summary>
    /// Set the health bar with a number of hearts based on health percent
    /// Every 20 hit points is equivalent to 1 heart
    /// </summary>
    /// <param name="healthEventArgs"></param>
    private void SetHealthBar(HealthEventArgs healthEventArgs)
    {
        ClearHealthBar();

        // Instantiate heart image prefabs
        int healthHearts = Mathf.CeilToInt(healthEventArgs.healthPercent * 100f / 20f);

        for (int i = 0; i < healthHearts; i++)
        {
            GameObject heart = Instantiate(GameResources.Instance.heartIconPrefab, transform);

            // Heart positioning on screen
            heart.GetComponent<RectTransform>().anchoredPosition = new Vector2(Settings.uiHeartIconSpacing * i, 0f);

            healthHeartsList.Add(heart);
        }
    }
}
