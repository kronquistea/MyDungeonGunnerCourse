using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    private void Start()
    {
        MusicManager.Instance.PlayMusic(GameResources.Instance.mainMenuMusic, 0.2f, 2f);
    }

    /// <summary>
    /// Called from the Play Game / Enter the Dungeon button
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene("MainGameScene");
    }
}
