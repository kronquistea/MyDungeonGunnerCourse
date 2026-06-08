using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenuUI : MonoBehaviour
{
    #region Header GAME AUDIO
    [Header("GAME AUDIO")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the music volume level text")]
    #endregion
    [SerializeField] private TextMeshProUGUI musicLevelText;

    #region Tooltip
    [Tooltip("Populate with the sounds volume level text")]
    #endregion
    [SerializeField] private TextMeshProUGUI soundsLevelText;

    private void Start()
    {
        // Initially hide the pause menu
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // Set the scale at which time passes to 0 (effectively freezing the game)
        Time.timeScale = 0f;

        StartCoroutine(InitializeUI());
    }

    private void OnDisable()
    {
        // Reenable scale at which time passes to normal level
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Quit and load main menu - linked to from pause menu UI
    /// </summary>
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
    
    /// <summary>
    /// Initialize the UI text
    /// </summary>
    /// <returns></returns>
    private IEnumerator InitializeUI()
    {
        // Wait for one frame to ensure that the previous music and sound level have been set
        yield return null;

        // Intiialize UI text
        soundsLevelText.SetText(SoundEffectManager.Instance.soundsVolume.ToString());
        musicLevelText.SetText(MusicManager.Instance.musicVolume.ToString());
    }

    /// <summary>
    /// Increase music volume - linked to from music volume increase button in pause menu UI
    /// </summary>
    public void IncreaseMusicVolume()
    {
        MusicManager.Instance.IncreaseMusicVolume();
        musicLevelText.SetText(MusicManager.Instance.musicVolume.ToString());
    }

    /// <summary>
    /// Decrease music volume - linked to from music volume decrease button in pause menu UI
    /// </summary>
    public void DecreaseMusicVolume()
    {
        MusicManager.Instance.DecreaseMusicVolume();
        musicLevelText.SetText(MusicManager.Instance.musicVolume.ToString());
    }

    /// <summary>
    /// Increase sounds volume - linked to from sounds volume increase button in pause menu UI
    /// </summary>
    public void IncreaseSoundsVolume()
    {
        SoundEffectManager.Instance.IncreaseSoundsVolume();
        soundsLevelText.SetText(SoundEffectManager.Instance.soundsVolume.ToString());
    }

    /// <summary>
    /// Decrease sounds volume - linked to from sounds volume decrease button in pause menu UI
    /// </summary>
    public void DecreaseSoundsVolume()
    {
        SoundEffectManager.Instance.DecreaseSoundsVolume();
        soundsLevelText.SetText(SoundEffectManager.Instance.soundsVolume.ToString());
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        // GAME AUDIO
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicLevelText), musicLevelText);
        HelperUtilities.ValidateCheckNullValue(this, nameof(soundsLevelText), soundsLevelText);
    }
#endif
    #endregion
}
