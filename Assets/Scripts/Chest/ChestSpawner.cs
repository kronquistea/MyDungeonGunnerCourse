using System.Collections.Generic;
using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    // Allow changing of chest spawn chance by dungeon level
    [System.Serializable]
    private struct RangeByLevel
    {
        public DungeonLevelSO dungeonLevel;
        [Range(0, 100)] public int min;
        [Range(0, 100)] public int max;
    }

    #region Header CHEST PREFAB
    [Header("CHEST PREFAB")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the chest prefab")]
    #endregion
    [SerializeField] private GameObject chestPrefab;

    #region Header CHEST SPAWN CHANCE
    [Space(10)]
    [Header("CHEST SPAWN CHANCE")]
    #endregion
    #region Tooltip
    [Tooltip("Minimum probablity for spawning a chest (in all dungeon levels - unless overriden)")]
    #endregion
    [SerializeField] [Range(0, 100)] private int chestSpawnChanceMin;

    #region Tooltip
    [Tooltip("maximum probablity for spawning a chest (in all dungeon levels - unless overriden)")]
    #endregion
    [SerializeField] [Range(0, 100)] private int chestSpawnChanceMax;

    #region Tooltip
    [Tooltip("Override chest spawn chance by level")]
    #endregion
    [SerializeField] private List<RangeByLevel> chestSpawnChanceByLevelList;

    #region Header CHEST SPAWN DETAILS
    [Space(10)]
    [Header("CHEST SPAWN DETAILS")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the desired chest spawn event (on room entry OR after defeating enemies)")]
    #endregion
    [SerializeField] private ChestSpawnEvent chestSpawnEvent;

    #region Tooltip
    [Tooltip("Populate with the chest spawn position (default spawner position OR near player position)")]
    #endregion
    [SerializeField] private ChestSpawnPosition chestSpawnPosition;

    #region Tooltip
    [Tooltip("The minimum number of items to spawn (not that a max of 1 of each type of health, ammo, and weapon will be spawned")]
    #endregion
    [SerializeField] [Range(0, 3)] private int numberOfItemsToSpawnMin;

    #region Tooltip
    [Tooltip("The maximum number of items to spawn (not that a max of 1 of each type of health, ammo, and weapon will be spawned")]
    #endregion
    [SerializeField] [Range(0, 3)] private int numberOfItemsToSpawnMax;

    #region Header CHEST CONTENT DETAILS
    [Space(10)]
    [Header("CHEST CONTENT DETAILS")]
    #endregion
    #region Tooltip
    [Tooltip("Range of health percentage to spawn for each level")]
    #endregion
    [SerializeField] private List<RangeByLevel> healthSpawnByLevelList;

    #region Tooltip
    [Tooltip("Range of ammo to percentage spawn for each level")]
    #endregion
    [SerializeField] private List<RangeByLevel> ammoSpawnByLevelList;

    #region Tooltip
    [Tooltip("Weapons to spawn for each dungeon level and their spawn ratios")]
    #endregion
    [SerializeField] private List<SpawnableObjectsByLevel<WeaponDetailsSO>> weaponSpawnByLevelList;

    private bool chestSpawned = false;
    private Room chestRoom;

    private void OnEnable()
    {
        // Subscribe to room changed event
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;

        // Subscribe to room enemies defeated event
        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;
    }

    private void OnDisable()
    {
        // Unsubscribe from room changed event
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;

        // Unsubscribe from room enemies defeated event
        StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;
    }

    /// <summary>
    /// Event handler for spawning a chest when the players enters a different room from the current room
    /// </summary>
    /// <param name="roomChangedEventArgs"></param>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        // Chest if the room is a chest room
        if (chestRoom == null)
        {
            // If the room is not a chest room then set the "chest room" to the non-chest room room 
            chestRoom = GetComponentInParent<InstantiatedRoom>().room;
        }

        // Check if the chest is not yet spawned in AND should be spawned in when the player enters the room AND the entered room is the room where the chest should be spawned
        if (!chestSpawned && chestSpawnEvent == ChestSpawnEvent.onRoomEntry && chestRoom == roomChangedEventArgs.room)
        {
            // Spawn the chest
            SpawnChest();
        }
    }

    /// <summary>
    /// Event handler for spawning a chest when the room enemies are defeated
    /// </summary>
    /// <param name="roomEnemiesDefeatedArgs"></param>
    private void StaticEventHandler_OnRoomEnemiesDefeated(RoomEnemiesDefeatedArgs roomEnemiesDefeatedArgs)
    {
        // Chest if the room is not technically a "chest room"
        if (chestRoom == null)
        {
            // If the room is not a chest room then set the "chestRoom" as a the current room
            chestRoom = GetComponentInParent<InstantiatedRoom>().room;
        }

        // Chest if the chest is not yet spawned in AND should be spawned in when the player defeats all the room enemies AND the room where the enemies were defeated is the current room
        if (!chestSpawned && chestSpawnEvent == ChestSpawnEvent.onEmemiesDefeated && chestRoom == roomEnemiesDefeatedArgs.room)
        {
            // Spawn the chest
            SpawnChest();
        }
    }

    /// <summary>
    /// Spawn a chest (maybe)
    /// </summary>
    private void SpawnChest()
    {
        chestSpawned = true;

        // I feel like this should be broken out separately (feels a bit weird to be in the method called "SpawnChest" event though this method might actually NOT spawn a chest)
        // Check if a chest should not be spawned in the room (based on the min and max spawn chance component variables)
        if (!RandomSpawnChest())
        {
            return;
        }

        // Calculate the number of items to spawn (1 item of each type max)
        GetItemsToSpawn(out int healthNum, out int ammoNum, out int weaponNum);

        // Instantiate a chest gameobject as a child of the chest spawner
        GameObject chestGameObject = Instantiate(chestPrefab, this.transform);

        // Check if the spawn position of the chest is at the default spawner position
        if (chestSpawnPosition == ChestSpawnPosition.atSpawnerPosition)
        {
            // Spawn the chest in the default spawner position
            chestGameObject.transform.position = this.transform.position;
        }
        // Else check if the spawn position of the chest is at the player position
        else if (chestSpawnPosition == ChestSpawnPosition.atPlayerPosition)
        {
            // Figure out spawn position based on player position
            Vector3 spawnPosition = HelperUtilities.GetSpawnPositionNearestToPlayer(GameManager.Instance.GetPlayer().transform.position);

            // Calculate a potential variation in the spawn location so the chest doesn't always spawn on top of the player (also allows for more than one chest to be spawned)
            Vector3 variation = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);

            // Set the spawn position of the chest to the player spawn position + the calculated variation in spawn position
            chestGameObject.transform.position = spawnPosition + variation;
        }

        // Get the actual chest component
        Chest chest = chestGameObject.GetComponent<Chest>();

        // Chest should not materialize if spawn event is on room entry. Chest should materialize if spawn event is on room enemies defeated
        bool materializeChest = chestSpawnEvent == ChestSpawnEvent.onRoomEntry ? false : true;

        // Initialize the chest
        chest.Initialize(materializeChest, GetHealthPercentToSpawn(healthNum), GetAmmoPercentToSpawn(ammoNum), GetWeaponDetailsToSpawn(weaponNum));
    }

    /// <summary>
    /// Test if a chest should be spawned based on the chest spawn chance
    /// </summary>
    /// <returns>True if a chest should be spawned, false otherwise</returns>
    private bool RandomSpawnChest()
    {
        // Calculate the actual chance that a chest should be spawned 
        int chancePercent = Random.Range(chestSpawnChanceMin, chestSpawnChanceMax + 1);
    
        // Check if an override chance percent is active for the current dungeon level
        foreach (RangeByLevel rangeByLevel in chestSpawnChanceByLevelList)
        {
            if (rangeByLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                chancePercent = Random.Range(rangeByLevel.min, rangeByLevel.max + 1);
                break;
            }
        }

        // Get a random value between 1 and 100
        int randomPercent = Random.Range(1, 101);

        // Check if the random value is within the spawn chance percent
        if (randomPercent <= chancePercent)
        {
            // Return true - a chest should be spawned
            return true;
        }
        else
        {
            // Return false - a chest should not be spawned
            return false;
        }
    }

    /// <summary>
    /// Calculate the number of items to spawn - max one item per type
    /// </summary>
    /// <param name="healthNum"></param>
    /// <param name="ammoNum"></param>
    /// <param name="weaponNum"></param>
    private void GetItemsToSpawn(out int healthNum, out int ammoNum, out int weaponNum)
    {
        healthNum = 0;
        ammoNum = 0;
        weaponNum = 0;

        // Determine how many items should be spawned (max one of each type)
        int numberOfItemsToSpawn = Random.Range(numberOfItemsToSpawnMin, numberOfItemsToSpawnMax + 1);

        // Variable to store exactly which item types should be spawned
        int choice;

        // Switch over the numberOfItemsToSpawn (0 items, 1 item, 2 items, or 3 items)
        switch (numberOfItemsToSpawn)
        {
            // Spawn no items
            case 0:
                return;

            // Spawn one item
            case 1:
                // Randomly pick which one item should be spawned
                choice = Random.Range(0, 3);
                if (choice == 0) { healthNum++; return; }
                if (choice == 1) { ammoNum++; return; }
                if (choice == 2) { weaponNum++; return; }
                return;

            // Spawn two items
            case 2:
                // Randomly pick which two items should be spawned
                choice = Random.Range(0, 3);
                if (choice == 0) { healthNum++; ammoNum++; return; }
                if (choice == 1) { ammoNum++; weaponNum++; return; }
                if (choice == 2) { weaponNum++; healthNum++; return; }
                return;

            // Spawn all three items
            case 3:
                // Spawn all three items
                healthNum++;
                ammoNum++;
                weaponNum++;
                return;

            // If no cases were entered (which should not happen), spawn no items
            default:
                //Debug.Log("Number of items to spawn was not between 0 and 3, therefore spawning no items!");
                return;
        }
    }

    /// <summary>
    /// Calculate health percent to spawn
    /// </summary>
    /// <param name="healthNum"></param>
    /// <returns>Integer representing percentage of health that should be replenished to the player</returns>
    private int GetHealthPercentToSpawn(int healthNum)
    {
        // If no health should be spawned, spawn none
        if (healthNum == 0)
        {
            return 0;
        }

        // Check spawn chances for the current level (by first looping through each spawn chance by level)
        foreach (RangeByLevel spawnPercentByLevel in healthSpawnByLevelList)
        {
            // Check if the spawn chance being checked is for the current dungeon level
            if (spawnPercentByLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                // Generate a random spawn chance based on component spawn chance variables
                return Random.Range(spawnPercentByLevel.min, spawnPercentByLevel.max);
            }
        }

        // Ensure something is returned
        return 0;
    }

    /// <summary>
    /// Calculate ammo percent to spawn
    /// </summary>
    /// <param name="ammoNum"></param>
    /// <returns>Integer representing percentage of ammo that should be replenished to the player</returns>
    private int GetAmmoPercentToSpawn(int ammoNum)
    {
        // If no ammo should be spawned, spawn none
        if (ammoNum == 0)
        {
            return 0;
        }

        // Check spawn chances for the current level (by first looping through each spawn chance by level)
        foreach (RangeByLevel spawnPercentByLevel in ammoSpawnByLevelList)
        {
            // Check if the spawn chance being checked is for the current dungeon level
            if (spawnPercentByLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                // Generate a random spawn chance based on component spawn chance variables
                return Random.Range(spawnPercentByLevel.min, spawnPercentByLevel.max);
            }
        }

        // Ensure something is returned
        return 0;
    }

    /// <summary>
    /// Get the weapon details to spawn
    /// </summary>
    /// <param name="weaponNum"></param>
    /// <returns>Weapon details if a weapon should be spawned and the player does not already have it, otherwise false</returns>
    private WeaponDetailsSO GetWeaponDetailsToSpawn(int weaponNum)
    {
        // Check if a weapon should be spawned
        if (weaponNum == 0)
        {
            return null;
        }

        // Create a random spawnable object of type WeaponDetailsSO that can be used to get a random weapon
        // This class is used to select a random item from a list based on the relative "ratios" of the item specified (think back to the spawn enemies functionality)
        RandomSpawnableObject<WeaponDetailsSO> weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(weaponSpawnByLevelList);

        // Get a random WeaponDetailsSO object
        WeaponDetailsSO weaponDetails = weaponRandom.GetItem();

        // Return the weapon details SO object
        return weaponDetails;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        // CHEST PREFAB
        HelperUtilities.ValidateCheckNullValue(this, nameof(chestPrefab), chestPrefab);

        // CHEST SPAWN CHANCE
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(chestSpawnChanceMin), chestSpawnChanceMin, nameof(chestSpawnChanceMax), chestSpawnChanceMax, true);
        if (chestSpawnChanceByLevelList != null && chestSpawnChanceByLevelList.Count > 0)
        {
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(chestSpawnChanceByLevelList), chestSpawnChanceByLevelList);

            foreach (RangeByLevel rangeByLevel in chestSpawnChanceByLevelList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(rangeByLevel.dungeonLevel), rangeByLevel.dungeonLevel);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(rangeByLevel.min), rangeByLevel.min, nameof(rangeByLevel.max), rangeByLevel.max, true);
            }
        }

        // CHEST SPAWN DETAILS
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(numberOfItemsToSpawnMin), numberOfItemsToSpawnMin, nameof(numberOfItemsToSpawnMax), numberOfItemsToSpawnMax, true);

        // CHEST CONTENT DETAILS
        if (weaponSpawnByLevelList != null && weaponSpawnByLevelList.Count > 0)
        {
            foreach (SpawnableObjectsByLevel<WeaponDetailsSO> weaponDetailsByLevel in weaponSpawnByLevelList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(weaponDetailsByLevel.dungeonLevel), weaponDetailsByLevel.dungeonLevel);
                
                foreach(SpawnableObjectRatio<WeaponDetailsSO> weaponRatio in weaponDetailsByLevel.spawnableObjectRatioList)
                {
                    HelperUtilities.ValidateCheckNullValue(this, nameof(weaponRatio.dungeonObject), weaponRatio.dungeonObject);
                    HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponRatio.ratio), weaponRatio.ratio, true);
                }
            }
        }
        if (healthSpawnByLevelList != null && healthSpawnByLevelList.Count > 0)
        {
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(healthSpawnByLevelList), healthSpawnByLevelList);

            foreach (RangeByLevel rangeByLevel in healthSpawnByLevelList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(rangeByLevel.dungeonLevel), rangeByLevel.dungeonLevel);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(rangeByLevel.min), rangeByLevel.min, nameof(rangeByLevel.max), rangeByLevel.max, true);
            }
        }
        if (ammoSpawnByLevelList != null && ammoSpawnByLevelList.Count > 0)
        {
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(ammoSpawnByLevelList), ammoSpawnByLevelList);

            foreach (RangeByLevel rangeByLevel in ammoSpawnByLevelList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(rangeByLevel.dungeonLevel), rangeByLevel.dungeonLevel);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(rangeByLevel.min), rangeByLevel.min, nameof(rangeByLevel.max), rangeByLevel.max, true);
            }
        }
    }
#endif
    #endregion
}
