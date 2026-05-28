using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GameManager : SingletonMonobehavior<GameManager>
{
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

        // Demo Code
        RoomEnemiesDefeated();
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

        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        Debug.Log("Boss Stage - Find and Destroy the Boss!");
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

        // Used for testing purposes (will later have proper UI)
        Debug.Log("Level Completed! Press enter/return to progress to the next level.");

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
    /// Process game won
    /// </summary>
    /// <returns>Coroutine</returns>
    private IEnumerator GameWon()
    {
        previousGameState = GameState.gameWon;

        Debug.Log("Game Won! All levels completed and bosses defeated! Game will restart in 10 seconds");

        // Wait for 10 seconds
        yield return new WaitForSeconds(10f);

        gameState = GameState.restartGame;
    }

    /// <summary>
    /// Process game lost
    /// </summary>
    /// <returns>Coroutine</returns>
    private IEnumerator GameLost()
    {
        previousGameState = GameState.gameLost;

        Debug.Log("Game Lost! Game will restart in 10 seconds!");

        // Wait for 10 seconds
        yield return new WaitForSeconds(10f);

        gameState = GameState.restartGame;
    }

    /// <summary>
    /// Restart the game by loading the main game scene
    /// </summary>
    private void RestartGame()
    {
        SceneManager.LoadScene("MainGameScene");
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
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }
#endif
    #endregion Validation
}
