using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Header("OBJECT REFERENCES")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the enter the dungeon play button gameobject")]
    #endregion
    [SerializeField] private GameObject playButton;

    #region Tooltip
    [Tooltip("Populate with the high scores button gameobject")]
    #endregion
    [SerializeField] private GameObject highScoresButton;

    #region Tooltip
    [Tooltip("Populate with the return to main menu button gameobject")]
    #endregion
    [SerializeField] private GameObject mainMenuButton;

    #region Tooltip
    [Tooltip("Populate with the instructions button gameobject")]
    #endregion
    [SerializeField] private GameObject instructionsButton;

    #region Tooltip
    [Tooltip("Populate with the quit button gameobject")]
    #endregion
    [SerializeField] private GameObject quitButton;

    private bool isInstructionsSceneLoaded = false;
    private bool isHighScoresSceneLoaded = false;

    private void Start()
    {
        MusicManager.Instance.PlayMusic(GameResources.Instance.mainMenuMusic, 0.2f, 2f);

        SceneManager.LoadScene("CharacterSelectorScene", LoadSceneMode.Additive);

        mainMenuButton.SetActive(false);
    }

    /// <summary>
    /// Called from the Play Game / Enter the Dungeon button
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene("MainGameScene");
    }

    /// <summary>
    /// Called from high scores button
    /// </summary>
    public void LoadHighScores()
    {
        playButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        quitButton.SetActive(false);
        isHighScoresSceneLoaded = true;

        SceneManager.UnloadSceneAsync("CharacterSelectorScene");

        mainMenuButton.SetActive(true);

        SceneManager.LoadScene("HighScoreScene", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Called from instructions button
    /// </summary>
    public void LoadInstructions()
    {
        playButton.SetActive(false);
        highScoresButton.SetActive(false);
        instructionsButton.SetActive(false);
        quitButton.SetActive(false);
        isInstructionsSceneLoaded = true;

        SceneManager.UnloadSceneAsync("CharacterSelectorScene");

        mainMenuButton.SetActive(true);

        SceneManager.LoadScene("InstructionsScene", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Called from return to main menu button
    /// </summary>
    public void LoadCharacterSelector()
    {
        mainMenuButton.SetActive(false);

        if (isHighScoresSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("HighScoreScene");
            isHighScoresSceneLoaded = false;
        }
        else if (isInstructionsSceneLoaded)
        {
            SceneManager.UnloadSceneAsync("InstructionsScene");
            isInstructionsSceneLoaded = false;
        }

        playButton.SetActive(true);
        highScoresButton.SetActive(true);
        instructionsButton.SetActive(true);
        quitButton.SetActive(true);

        SceneManager.LoadScene("CharacterSelectorScene", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Quit the game when the game is being played OUTSIDE of the Unity editor
    /// </summary>
    public void QuitGame()
    {
        // Quit the application
        Application.Quit();
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        // OBJECT REFERENCES
        HelperUtilities.ValidateCheckNullValue(this, nameof(playButton), playButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(highScoresButton), highScoresButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(mainMenuButton), mainMenuButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(instructionsButton), instructionsButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(quitButton), quitButton);
    }
#endif
    #endregion
}
