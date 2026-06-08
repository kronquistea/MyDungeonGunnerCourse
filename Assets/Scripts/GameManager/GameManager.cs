using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameManager : SingletonMonobehavior<GameManager>
{
    #region Header GAMEOBJECT REFERENCES
    [Space(10)]
    [Header("GAMEOBJECT REFERENCES")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the MessageText text mesh pro component in the FadeScreenUI")]
    #endregion
    [SerializeField] private TextMeshProUGUI messageTextTMP;

    #region Tooltip
    [Tooltip("Populate with pause menu gameobject in hierarchy")]
    #endregion
    [SerializeField] private GameObject pauseMenu;

    #region Tooltip
    [Tooltip("Populate with the FadeImage canvas group component in the FadeScreenUI")]
    #endregion
    [SerializeField] private CanvasGroup canvasGroup;

    #region Header DUNGEON LEVELS
    [Space(10)]
    [Header("DUNGEON LEVELS")]
    #endregion Header DUNGEON LEVELS
    #region Tooltip
    [Tooltip("Populate with the dungeon level scriptable object")]
    #endregion Tooltip
    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;

    #region Tooltip
    [Tooltip("Populate with the starting dungeon level for test, first level = 0")]
    #endregion Tooltip
    [SerializeField] private int currentDungeonLevelListIndex = 0;

    private Room currentRoom;
    private Room previousRoom;

    private PlayerDetailsSO playerDetails;
    private Player player;

    [HideInInspector] public GameState gameState;
    [HideInInspector] public GameState previousGameState;

    private long gameScore;
    private int scoreMultiplier;
    private const int scoreMultiplierMin = 1;
    private const int scoreMultiplierMax = 30;

    private InstantiatedRoom bossRoom;
    private bool skullIconAddedToMap = false;

    private bool isFading = false;

    protected override void Awake()
    {
        // Call base class
        base.Awake();

        // Set player details - saved in current player scriptable object from the main menu
        playerDetails = GameResources.Instance.currentPlayer.playerDetails;

        InstantiatePlayer();
    }

    /// <summary>
    /// Create player in scene at position
    /// </summary>
    private void InstantiatePlayer()
    {
        // Instantiate player
        GameObject playerGameObject = Instantiate(playerDetails.playerPrefab);

        // Initialize player
        player = playerGameObject.GetComponent<Player>();

        player.Initialize(playerDetails);
    }

    private void OnEnable()
    {
        // Subscribe to room changed event
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;

        // Subscribe to room enemies defeated event
        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;

        // Subscribe to points scored event
        StaticEventHandler.OnPointsScored += StaticEventHandler_OnPointsScored;

        // Subscribe to multplier event
        StaticEventHandler.OnMultiplier += StaticEventHandler_OnMultiplier;

        // Subscribe to player destroyed event
        player.destroyedEvent.OnDestroyed += Player_OnDestroyed;
    }

    private void OnDisable()
    {
        // Unsubscribe from room changed event
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;

        // Unsubscribe from room enemies defeated event
        StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;

        // Unsubscribe from on points scored event
        StaticEventHandler.OnPointsScored -= StaticEventHandler_OnPointsScored;

        // Unsubscribe from multplier event
        StaticEventHandler.OnMultiplier -= StaticEventHandler_OnMultiplier;

        // Unsubscribe from player destroyed event
        player.destroyedEvent.OnDestroyed -= Player_OnDestroyed;
    }

    /// <summary>
    /// Handle room changed event
    /// </summary>
    /// <param name="roomChangedEventArgs"></param>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        if (!skullIconAddedToMap)
        {
            foreach (string roomID in roomChangedEventArgs.room.childRoomIDList)
            {
                if (DungeonBuilder.Instance.dungeonBuilderRoomDictionary[roomID].roomNodeType.isBossRoom)
                {
                    bossRoom = DungeonBuilder.Instance.dungeonBuilderRoomDictionary[roomID].instantiatedRoom;

                    AddSkullIconToMap();

                    bossRoom = null;
                }
            }
        }

        SetCurrentRoom(roomChangedEventArgs.room);
    }

    /// <summary>
    /// Handle room enemies defeated event
    /// </summary>
    /// <param name="roomEnemiesDefeatedArgs"></param>
    private void StaticEventHandler_OnRoomEnemiesDefeated(RoomEnemiesDefeatedArgs roomEnemiesDefeatedArgs)
    {
        RoomEnemiesDefeated();
    }

    /// <summary>
    /// Handle points scored event
    /// </summary>
    /// <param name="pointsScoredArgs"></param>
    private void StaticEventHandler_OnPointsScored(PointsScoredArgs pointsScoredArgs)
    {
        // Increase score
        gameScore += pointsScoredArgs.points * scoreMultiplier;

        // Call score changed event
        StaticEventHandler.CallScoreChangedEvent(gameScore, scoreMultiplier);
    }

    /// <summary>
    /// Handle multiplier event
    /// </summary>
    /// <param name="multiplierArgs"></param>
    private void StaticEventHandler_OnMultiplier(MultiplierArgs multiplierArgs)
    {
        // If the multiplier is true (enemy hit) increase multiplier by 1
        if (multiplierArgs.multiplier)
        {
            scoreMultiplier++;
        }
        else
        {
            scoreMultiplier--;
        }

        // Clamp score multiplier to min and max values (currently 1 and 30)
        scoreMultiplier = Mathf.Clamp(scoreMultiplier, scoreMultiplierMin, scoreMultiplierMax);

        StaticEventHandler.CallScoreChangedEvent(gameScore, scoreMultiplier);
    }

    /// <summary>
    /// Handle player destroyed event
    /// </summary>
    /// <param name="destroyedEvent"></param>
    /// <param name="destroyedEventArgs"></param>
    private void Player_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        previousGameState = gameState;
        gameState = GameState.gameLost;
    }

    // Start is called before the first frame update
    private void Start()
    {
        previousGameState = GameState.gameStarted;
        gameState = GameState.gameStarted;

        gameScore = 0;

        scoreMultiplier = 1;

        // Set screen to black
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));
    }

    // Update is called once per frame
    private void Update()
    {
        HandleGameState();

        // For testing purposes
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    gameState = GameState.gameStarted;
        //}
    }

    /// <summary>
    /// Handle game state
    /// </summary>
    private void HandleGameState()
    {
        // Handle game state
        switch (gameState)
        {
            case GameState.gameStarted:
                PlayDungeonLevel(currentDungeonLevelListIndex); // Play first level
                gameState = GameState.playingLevel;
                RoomEnemiesDefeated(); // Avoid issues if only rooms in dungeon are entrance, corridor, boss room
                break;

            case GameState.playingLevel:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGameMenu();
                }

                // Check if the player is holding the tab key
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    DisplayDungeonOverviewMap();
                }
                break;

            case GameState.engagingEnemies:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGameMenu();
                }
                break;

            case GameState.dungeonOverviewMap:
                // Check if the play released the tab key
                if (Input.GetKeyUp(KeyCode.Tab))
                {
                    DungeonMap.Instance.ClearDungeonOverviewMap();
                }
                break;

            case GameState.bossStage: // Enable the player to open the map after clearing all rooms, but before fighting the boss
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGameMenu();
                }

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    DisplayDungeonOverviewMap();
                }
                break;

            case GameState.engagingBoss:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGameMenu();
                }
                break;

            case GameState.levelCompleted:
                StartCoroutine(LevelCompleted()); // Display level completed text
                break;

            case GameState.gameWon:
                // Only process game won one time
                if (previousGameState != GameState.gameWon)
                {
                    StartCoroutine(GameWon());
                }
                break;

            case GameState.gameLost:
                // Only process game lost one time
                if (previousGameState != GameState.gameLost)
                {
                    StopAllCoroutines(); // Prevent messages if the level is cleared just as player death occurs
                    StartCoroutine(GameLost());
                }
                break;

            case GameState.restartGame:
                RestartGame();
                break;

            case GameState.gamePaused:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGameMenu();
                }
                break;
        }
    }

    /// <summary>
    /// Set the current room the player is in
    /// </summary>
    /// <param name="room"></param>
    public void SetCurrentRoom(Room room)
    {
        previousRoom = currentRoom;
        currentRoom = room;

        //// Debug
        // Debug.log(room.prefab.name.ToString());
    }

    /// <summary>
    /// Add the skull icon to the map to guide the player to the boss room
    /// </summary>
    private void AddSkullIconToMap()
    {
        if (!skullIconAddedToMap)
        {
            GameObject bossRoomDoor = bossRoom.GetComponentInChildren<Door>().gameObject;

            GameObject skullIcon = Instantiate(GameResources.Instance.minimapSkullPrefab, bossRoom.transform);

            skullIcon.transform.localPosition = bossRoomDoor.transform.localPosition;
        }

        skullIconAddedToMap = true;
    }

    /// <summary>
    /// Room enemies defeated - test if all dungeon rooms have been cleared of enemies.
    /// If so, load next dungeon game level.
    /// </summary>
    private void RoomEnemiesDefeated()
    {
        // Initialize dungeon as being cleared - then test each room
        bool isDungeonClearOfRegularEnemies = true;
        bossRoom = null;

        // Loop through all dungeon rooms, checking if each one is clear
        foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            // Check if the current room is a boss room
            if (keyValuePair.Value.roomNodeType.isBossRoom)
            {
                // Set the bossRoom and continue to the next loop iteration
                bossRoom = keyValuePair.Value.instantiatedRoom;
                continue;
            }

            // Check if dungeon room still has enemies
            if (!keyValuePair.Value.isClearedOfEnemies)
            {
                // Dungeon is not yet cleared, so break
                isDungeonClearOfRegularEnemies = false;
                break;
            }
        }

        // Set Game State
        if ((isDungeonClearOfRegularEnemies && bossRoom == null) || (isDungeonClearOfRegularEnemies && bossRoom.room.isClearedOfEnemies))
        {
            // Check if there are still more dungeon levels to be played
            if (currentDungeonLevelListIndex < dungeonLevelList.Count - 1)
            {
                gameState = GameState.levelCompleted;
            }
            // Else the game has been won
            else
            {
                gameState = GameState.gameWon;
            }
        }
        // Else check if the dungeon is clear of enemies (except the boss room)
        else if (isDungeonClearOfRegularEnemies)
        {
            // Set game state to boss stage (only remaining thing in the level is the boss)
            gameState = GameState.bossStage;

            // Start the BossStage coroutine
            StartCoroutine(BossStage());
        }
    }

    public void PauseGameMenu()
    {
        // Check if the game is not already paused
        if (gameState != GameState.gamePaused)
        {
            pauseMenu.SetActive(true);

            GetPlayer().playerControl.DisablePlayer();

            previousGameState = gameState;
            gameState = GameState.gamePaused;
        }
        // Else check if the game is already paused
        else if (gameState == GameState.gamePaused)
        {
            pauseMenu.SetActive(false);

            GetPlayer().playerControl.EnablePlayer();

            gameState = previousGameState;
            previousGameState = GameState.gamePaused;
        }
    }

    /// <summary>
    /// Display the dungeon overview map
    /// </summary>
    private void DisplayDungeonOverviewMap()
    {
        // Check if the screen is currently fading
        if (isFading)
        {
            // If so, do not allow the player to open the map
            return;
        }

        // Open the dungeon overview map
        DungeonMap.Instance.DisplayDungeonOverviewmap();
    }

    /// <summary>
    /// Play dungeon level
    /// </summary>
    /// <param name="dungeonLevelListIndex"></param>
    private void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        // Build dungeon for level
        bool dungeonBuiltSuccessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);

        if (!dungeonBuiltSuccessfully)
        {
            Debug.LogError("Could not build dungeon from specified rooms and node graphs");
        }

        // Call static event that room has changed
        StaticEventHandler.CallRoomChangedEvent(currentRoom);

        // Set player roughly mid-room
        player.gameObject.transform.position = new Vector3(
            (currentRoom.lowerBounds.x + currentRoom.upperBounds.x) / 2f,
            (currentRoom.lowerBounds.y + currentRoom.upperBounds.y) / 2f,
            0f);

        // Get nearest spawn point in room nearest to player
        player.gameObject.transform.position = HelperUtilities.GetSpawnPositionNearestToPlayer(player.gameObject.transform.position);

        StartCoroutine(DisplayDungeonLevelText());

        // Demo Code
        //RoomEnemiesDefeated();
    }

    /// <summary>
    /// Display dungeon level text
    /// </summary>
    /// <returns>Coroutine</returns>
    private IEnumerator DisplayDungeonLevelText()
    {
        // Set screen to black
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));

        // Stop player movement
        GetPlayer().playerControl.DisablePlayer();

        // ex. LEVEL 1 \n\n The Hall of Heroes
        string messageText = "LEVEL " + (currentDungeonLevelListIndex + 1).ToString() + "\n\n" + dungeonLevelList[currentDungeonLevelListIndex].levelName.ToUpper();

        // Display the message before allowing player to move and fade main game in
        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 2f));

        // Allow player movement
        GetPlayer().playerControl.EnablePlayer();

        // Fade in main game
        yield return StartCoroutine(Fade(1f, 0f, 2f, Color.black));
    }

    private IEnumerator DisplayMessageRoutine(string text, Color textColor, float displaySeconds)
    {
        messageTextTMP.SetText(text);
        messageTextTMP.color = textColor;

        // Check if the text should be displayed for any amount of time
        if (displaySeconds > 0f)
        {
            // Variable to manipulate remaining time
            float timer = displaySeconds;

            // Continue showing text until timer is 0 or user pressed enter (to skip past text)
            while (timer > 0f && !Input.GetKeyDown(KeyCode.Return))
            {
                // Reduce timer
                timer -= Time.deltaTime;
                
                // Wait until next frame
                yield return null;
            }
        }
        // Else the message should be displayed until the enter key is pressed
        else
        {
            while (!Input.GetKeyDown(KeyCode.Return))
            {
                yield return null;
            }
        }

        yield return null;

        // Clear text
        messageTextTMP.SetText("");
    }

    /// <summary>
    /// Enter boss stage - set boss room as active, unlock boss room doors, wait for 2 seconds
    /// </summary>
    /// <returns>Coroutine</returns>
    private IEnumerator BossStage()
    {
        // Activate the boss room
        bossRoom.gameObject.SetActive(true);

        // Unlock boss room doors with no delay
        bossRoom.UnlockDoors(0f);

        if (!skullIconAddedToMap)
        {
            AddSkullIconToMap();
        }

        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Fade in canvas to display message
        yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        // Show boss stage message
        string message = "WELL DONE " + GameResources.Instance.currentPlayer.playerName + "! YOU'VE SURVIVED ...SO FAR\n\nNOW FIND AND DEFEAT THE BOSS....GOOD LUCK!";
        yield return StartCoroutine(DisplayMessageRoutine(message, Color.white, 5f));

        // Fade out canvas to display game
        yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));
    }

    /// <summary>
    /// Show level as being completed - load next level
    /// </summary>
    /// <returns>Coroutine</returns>
    private IEnumerator LevelCompleted()
    {
        gameState = GameState.playingLevel;

        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Fade in canvas to display message
        yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        // Show level completed message
        string levelCompletedMessage = "WELL DONE " + GameResources.Instance.currentPlayer.playerName + "! YOU'VE SURVIVED THIS DUNGEON LEVEL!";
        yield return StartCoroutine(DisplayMessageRoutine(levelCompletedMessage, Color.white, 5f));

        // Show further action message
        string furtherActionMessage = "COLLECT ANY LOOT ...THEN PRESS ENTER\n\nTO DESCEND FURTHER INTO THE DUNGEON";
        yield return StartCoroutine(DisplayMessageRoutine(furtherActionMessage, Color.white, 5f));

        // Fade out canvas to display game
        yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        // Wait until user presses enter/return key
        while (!Input.GetKeyDown(KeyCode.Return))
        {
            yield return null;
        }

        // Avoid enter being detected twice (starting next level and skipping text/dialogue potential issues)
        yield return null;

        // Next level
        currentDungeonLevelListIndex++;

        // play next dungeon level
        PlayDungeonLevel(currentDungeonLevelListIndex);
    }

    /// <summary>
    /// Fade canvas group
    /// </summary>
    /// <param name="startFadeAlpha"></param>
    /// <param name="targetFadeAlpha"></param>
    /// <param name="fadeSeconds"></param>
    /// <param name="backgroundColor"></param>
    /// <returns>Coroutine</returns>
    public IEnumerator Fade(float startFadeAlpha, float targetFadeAlpha, float fadeSeconds, Color backgroundColor)
    {
        // Signal to not allow opening dungeon overview map while the screen is fading
        isFading = true;

        Image image = canvasGroup.GetComponent<Image>();
        image.color = backgroundColor;

        float time = 0;
        
        // Loop until the desired amount of time for the fade has elapsed
        while(time <= fadeSeconds)
        {
            // Increase time tracker
            time += Time.deltaTime;

            // Set the alpha of the canvas group to the linear interpretation between the starting alpha and target alpha
            // based on how much time has passed relative to the desired fade time
            canvasGroup.alpha = Mathf.Lerp(startFadeAlpha, targetFadeAlpha, time / fadeSeconds);

            // Wait until next frame
            yield return null;
        }

        // Signal to re-allow opening dungeon overview map after the screen is done fading
        isFading = false;
    }

    /// <summary>
    /// Process game won
    /// </summary>
    /// <returns>Coroutine</returns>
    private IEnumerator GameWon()
    {
        previousGameState = GameState.gameWon;

        // Disable the player
        GetPlayer().playerControl.DisablePlayer();

        // Get player rank
        int rank = HighScoreManager.Instance.GetRank(gameScore);

        string rankText;

        // Check if the player's score is ranked in the top number of high scores to save
        if (rank > 0 && rank <= Settings.numberOfHighScoresToSave)
        {
            // Set rank text to be displayed to player
            rankText = "YOUR SCORE IS RANKED " + rank.ToString("#0") + " IN THE TOP " + Settings.numberOfHighScoresToSave.ToString("#0");

            string name = GameResources.Instance.currentPlayer.playerName;

            if (name == "")
            {
                name = playerDetails.playerCharacterName.ToUpper();
            }

            HighScoreManager.Instance.AddScore(
                new Score() 
                    { 
                        playerName = name, 
                        levelDescription = "LEVEL " + (currentDungeonLevelListIndex + 1).ToString() + " - " + GetCurrentDungeonLevel().levelName.ToUpper(), 
                        playerScore = gameScore 
                    }, 
                    rank
            );
        }
        else
        {
            rankText = "YOUR SCORE ISN'T RANKED IN THE TOP " + Settings.numberOfHighScoresToSave.ToString("#0");
        }

        // Wait for 1 second
        yield return new WaitForSeconds(1f);

        // Fade in canvas to display message
        yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

        // Show game won message
        string gameWonMessage = "WELL DONE " + GameResources.Instance.currentPlayer.playerName + "! YOU'VE SURVIVED THE ENTIRE DUNGEON!";
        yield return StartCoroutine(DisplayMessageRoutine(gameWonMessage, Color.white, 5f));

        // Show score message
        string scoreMessage = "YOUR SCORE: " + gameScore.ToString("###,###0") + "\n\n" + rankText;
        yield return StartCoroutine(DisplayMessageRoutine(scoreMessage, Color.white, 4f));

        // Show further action message
        string furtherActionMessage = "PRESS ENTER TO RESTART THE GAME";
        yield return StartCoroutine(DisplayMessageRoutine(furtherActionMessage, Color.white, 0f));

        gameState = GameState.restartGame;
    }

    /// <summary>
    /// Process game lost
    /// </summary>
    /// <returns>Coroutine</returns>
    private IEnumerator GameLost()
    {
        previousGameState = GameState.gameLost;

        // Disable the player
        GetPlayer().playerControl.DisablePlayer();

        // Get player rank
        int rank = HighScoreManager.Instance.GetRank(gameScore);

        string rankText;

        // Check if the player's score is ranked in the top number of high scores to save
        if (rank > 0 && rank <= Settings.numberOfHighScoresToSave)
        {
            // Set rank text to be displayed to player
            rankText = "YOUR SCORE IS RANKED " + rank.ToString("#0") + " IN THE TOP " + Settings.numberOfHighScoresToSave.ToString("#0");

            string name = GameResources.Instance.currentPlayer.playerName;

            if (name == "")
            {
                name = playerDetails.playerCharacterName.ToUpper();
            }

            HighScoreManager.Instance.AddScore(
                new Score()
                {
                    playerName = name,
                    levelDescription = "LEVEL " + (currentDungeonLevelListIndex + 1).ToString() + " - " + GetCurrentDungeonLevel().levelName.ToUpper(),
                    playerScore = gameScore
                },
                    rank
            );
        }
        else
        {
            rankText = "YOUR SCORE ISN'T RANKED IN THE TOP " + Settings.numberOfHighScoresToSave.ToString("#0");
        }

        // Wait for 1 second
        yield return new WaitForSeconds(1f);

        // Fade in canvas to display message
        yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

        // Disable enemies (FindObjectsOfType is resource intensive - but since it is the end of the game it should be fine)
        Enemy[] enemyArray = GameObject.FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemyArray)
        {
            enemy.gameObject.SetActive(false);
        }

        // Show game lost message
        string gameLostMessage = "BAD LUCK " + GameResources.Instance.currentPlayer.playerName + "! YOU HAVE SUCCUMBED TO THE DUNGEON!";
        yield return StartCoroutine(DisplayMessageRoutine(gameLostMessage, Color.white, 5f));

        // Show score message
        string scoreMessage = "YOUR SCORE: " + gameScore.ToString("###,###0") + "\n\n" + rankText;
        yield return StartCoroutine(DisplayMessageRoutine(scoreMessage, Color.white, 4f));

        // Show further action message
        string furtherActionMessage = "PRESS ENTER TO RESTART THE GAME";
        yield return StartCoroutine(DisplayMessageRoutine(furtherActionMessage, Color.white, 0f));

        gameState = GameState.restartGame;
    }

    /// <summary>
    /// Restart the game by loading the main game scene
    /// </summary>
    private void RestartGame()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// Get the current room the player is in
    /// </summary>
    /// <returns>Current room</returns>
    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    /// <summary>
    /// Get the player
    /// </summary>
    /// <returns></returns>
    public Player GetPlayer()
    {
        return player;
    }

    /// <summary>
    /// Get the player minimap icon
    /// </summary>
    /// <returns></returns>
    public Sprite GetPlayerMiniMapIcon()
    {
        return playerDetails.playerMiniMapIcon;
    }

    /// <summary>
    /// Get the current dungeon level
    /// </summary>
    /// <returns>DungeonLevelSO for the current dungeon level</returns>
    public DungeonLevelSO GetCurrentDungeonLevel()
    {
        return dungeonLevelList[currentDungeonLevelListIndex];
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        // OBJECT REFERENCES
        HelperUtilities.ValidateCheckNullValue(this, nameof(messageTextTMP), messageTextTMP);
        HelperUtilities.ValidateCheckNullValue(this, nameof(pauseMenu), pauseMenu);
        HelperUtilities.ValidateCheckNullValue(this, nameof(canvasGroup), canvasGroup);
        
        // DUNGEON LEVELS
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }
#endif
    #endregion Validation
}
