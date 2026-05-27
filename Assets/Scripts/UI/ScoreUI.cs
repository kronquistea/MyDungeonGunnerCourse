using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    private TextMeshProUGUI scoreTextTMP;

    private void Awake()
    {
        // Load components
        scoreTextTMP = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        // Subscribe to score changed event
        StaticEventHandler.OnScoreChanged += StaticEventHandler_OnScoreChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from score changed event
        StaticEventHandler.OnScoreChanged -= StaticEventHandler_OnScoreChanged;
    }

    /// <summary>
    /// Handle score changed event
    /// </summary>
    /// <param name="scoreChangedArgs"></param>
    private void StaticEventHandler_OnScoreChanged(ScoreChangedArgs scoreChangedArgs)
    {
        scoreTextTMP.text = "SCORE: " + scoreChangedArgs.score.ToString("###,###0");
    }
}
