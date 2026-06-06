using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    private void Start()
    {
        // Initially hide the pause menu
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // Set the scale at which time passes to 0 (effectively freezing the game)
        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        // Reenable scale at which time passes to normal level
        Time.timeScale = 1f;
    }
}
