using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonobehavior<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccessful;

    /// <summary>
    /// Override SingletonMonobehavior Awake method
    /// </summary>
    protected override void Awake()
    {
        // Still execute code in SingletonMonobehavior Awake method
        base.Awake();

        // Load the room node type list
        LoadRoomNodeTypeList();

        // Set dimmed material to fully visible
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    }

    /// <summary>
    /// Load the room node type list
    /// </summary>
    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Generate random dungeon
    /// </summary>
    /// <param name="currentDungeonLevel"></param>
    /// <returns>true if dungeon built, false if failed</returns>
    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList;

        // Load the scriptable object room templates into the dictionary
        LoadRoomTemplatesIntoDictionary();

        dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;

        while (!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            // Select a random room node graph from the list
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            int dungeonRebuildAttemptsForNodeGraph = 0;
            dungeonBuildSuccessful = false;

            // Loop until dungeon successfully built or more than max attempts for each node graph
            while (!dungeonBuildSuccessful && dungeonRebuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForRoomGraph)
            {
                // Clear dungeon room game objets and dungeon room dictionary
                ClearDungeon();

                dungeonRebuildAttemptsForNodeGraph++;

                // Attempt to build a random dungeon for the selected room node graph
                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);
            }

            if (dungeonBuildSuccessful)
            {
                // Instantiate room game objects
                InstantiateRoomGameobjects();
            }
        }

        return dungeonBuildSuccessful;
    }

    /// <summary>
    /// Load the room templates into the dictionary
    /// </summary>
    private void LoadRoomTemplatesIntoDictionary()
    {
        // Clear room template dictionary
        roomTemplateDictionary.Clear();

        // Load room template list into dictionary
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                Debug.Log("Duplicate Room Template Key In " + roomTemplateList);
            }
        }
    }

    /// <summary>
    /// Attempt to randomly build the dungeon for the specified roomNodeGraph
    /// </summary>
    /// <param name="roomNodeGraph"></param>
    /// <returns>True if layout was successful, false otherwise</returns>
    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        // Create open room node queue
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        // Add entrance node to room node queue from room node graph
        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

        if (entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log("No Entrance Node");
            return false; // Dungeon not built
        }

        // Start with no room overlaps
        bool noRoomOverlaps = true;

        // Process open room nodes queue
        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        // If all the room nodes have been processed and there hasn't been a room overlap then return true;
        if (openRoomNodeQueue.Count == 0 && noRoomOverlaps)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Process rooms in the open room node queue
    /// </summary>
    /// <param name="roomNodeGraph"></param>
    /// <param name="openRoomNodeQueue"></param>
    /// <param name="noRoomOverlaps"></param>
    /// <returns>True if there are no room overlaps, false otherwise</returns>
    private bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        // While room nodes in open room queue and no room overlaps detected
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            // Get next room node from open room node queue
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            // Add child nodes to queue from room node graph (with links to this parent room)
            foreach (RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            // If the room is the entrnace mark as positioned and add to room dictionary
            if (roomNode.roomNodeType.isEntrance)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                room.isPositioned = true;

                // Add room to room dictionary
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            // else if the room type is NOT an entrance
            else
            {
                // Else get parent room for node
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

                // See if room can be placed without overlaps
                noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
            }
        }

        return noRoomOverlaps;
    }

    /// <summary>
    /// Attempt to place the room node in the dungeon
    /// </summary>
    /// <param name="roomNode"></param>
    /// <param name="parentRoom"></param>
    /// <returns>Room if the room can be placed, null otherwise</returns>
    private bool CanPlaceRoomWithNoOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        // Initialize and assume overlap until proven otherwise
        bool roomOverlaps = true;

        // Try to place against all available doorways of the parent until the room is succesfully placed without overlap
        while (roomOverlaps)
        {
            // Select random unconnected available doorway for parent
            List<Doorway> unconnectedAvailalbeParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorwayList).ToList();

            if (unconnectedAvailalbeParentDoorways.Count == 0)
            {
                // If no more doorways to try then return overlap failure
                return false; // room overlaps
            }

            Doorway doorwayParent = unconnectedAvailalbeParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailalbeParentDoorways.Count)];

            // Get a random room template for room node that is consistent with the parent door orientation
            RoomTemplateSO roomTemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            // Create a room
            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

            // Place the room - returns true if the room does NOT overlap
            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                // If room doesn't overlap, then set to false to exit loop
                roomOverlaps = false;

                // Mark room as positioned
                room.isPositioned = true;

                // Add room to dictionary
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                roomOverlaps = true;
            }
        }

        return true; // No room overlaps
    }

    /// <summary>
    /// Get random room template for room node taking into account the parent doorway orientation
    /// </summary>
    /// <param name="roomNode"></param>
    /// <param name="doorwayParent"></param>
    /// <returns></returns>
    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomTemplate = null;

        // If room node is a corridor then select random correct corridor room template based on parent doorway orientation
        if (roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;

                case Orientation.east:
                case Orientation.west:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorrdorEW));
                    break;

                case Orientation.none:
                    break;

                default:
                    break;
            }
        }
        else // Else select random room template
        {
            roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }

        return roomTemplate;
    }

    /// <summary>
    /// Place the room
    /// </summary>
    /// <param name="parentRoom"></param>
    /// <param name="doorwayParent"></param>
    /// <param name="room"></param>
    /// <returns>True if the room does NOT overlap, false otherwise</returns>
    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        // Get current room doorway position
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorwayList);

        // Return if no doorway in room opposite to parent doorway
        if (doorway == null)
        {
            // Just mark the parent doorway as unavailable so we don't try and connect it again
            doorwayParent.isUnavailable = true;

            return false;
        }

        // Calculate 'world' grid parent doorway position
        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        Vector2Int adjustment = Vector2Int.zero;

        // Calculate adjustment offset based on room doorway position that we are trying to connect (eg if this doorway is west then
        // we need to add (1,0) to the east parent doorway

        switch (doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1);
                break;

            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;

            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;

            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;

            case Orientation.none:
                break;

            default:
                break;
        }

        // Calculate room lower bounds and upper bounds based on positioning to align with parent doorway
        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room overlappingRoom = CheckForRoomOverlap(room);

        if (overlappingRoom == null)
        {
            // Mark dooways as connected and unavailable
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;

            doorway.isConnected = true;
            doorway.isUnavailable = true;

            // Return true to show rooms have been connected with no overlap
            return true;
        }
        else
        {
            // Mark the parent doorway as unavailable so we don't try and connect it again
            doorwayParent.isUnavailable = true;

            return false;
        }
    }

    /// <summary>
    /// Get the doorway from the doorway list that has the opposite orientation to doorway
    /// </summary>
    /// <param name="parentDoorway"></param>
    /// <param name="doorwayList"></param>
    /// <returns>Doorway to check that is in opposite orientation to parentDoorway</returns>
    private Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorwayList)
    {
        foreach (Doorway doorwayToCheck in doorwayList)
        {
            if (parentDoorway.orientation == Orientation.east && doorwayToCheck.orientation == Orientation.west)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.west && doorwayToCheck.orientation == Orientation.east)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.north && doorwayToCheck.orientation == Orientation.south)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.south && doorwayToCheck.orientation == Orientation.north)
            {
                return doorwayToCheck;
            }
        }
        return null;
    }

    /// <summary>
    /// Check for room that overlap the upper and lower bounds parameters
    /// </summary>
    /// <param name="roomToTest"></param>
    /// <returns>Room if there are overlapping rooms, null otherwise</returns>
    private Room CheckForRoomOverlap(Room roomToTest)
    {
        // Iterate through all rooms
        foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            // Skip if the same room as room to test or room has NOT been positioned
            if (room.id == roomToTest.id || !room.isPositioned)
            {
                continue;
            }

            // If room overlaps
            if (IsOverLappingRoom(roomToTest, room))
            {
                return room;
            }
        }

        // Ideal case - we can place the room without it overlapping
        return null;
    }

    /// <summary>
    /// Check if two rooms overlap each other
    /// </summary>
    /// <param name="room1"></param>
    /// <param name="room2"></param>
    /// <returns>True if they overlap, false otherwise</returns>
    private bool IsOverLappingRoom(Room room1, Room room2)
    {
        bool isOverlappingX = IsOverLappingInterval(room1.lowerBounds.x, room1.upperBounds.x, room2.lowerBounds.x, room2.upperBounds.x);
        bool isOverlappingY = IsOverLappingInterval(room1.lowerBounds.y, room1.upperBounds.y, room2.lowerBounds.y, room2.upperBounds.y);

        if (isOverlappingX && isOverlappingY)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    /// <summary>
    /// Check if interval 1 overlaps interval 2
    /// </summary>
    /// <param name="imin1"></param>
    /// <param name="imax1"></param>
    /// <param name="imin2"></param>
    /// <param name="imax2"></param>
    /// <returns>True if overlapping on specific axis (x OR y, not x AND y), false otherwise</returns>
    private bool IsOverLappingInterval(int imin1, int imax1, int imin2, int imax2)
    {
        if (Mathf.Max(imin1, imin2) <= Mathf.Min(imax2, imax2))
        {
            return true;
        }
        else // Ideal case
        {
            return false;
        }
    }

    /// <summary>
    /// Get a random room template from the roomtemplatelist that matches the roomType and return it
    /// </summary>
    /// <param name="roomNodeType"></param>
    /// <returns>Null if no matching room templates found</returns>
    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        // Loop through room template list
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            // Add matching room templates
            if (roomTemplate.roomNodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        // Return null if list is zero
        if (matchingRoomTemplateList.Count == 0)
        {
            return null;
        }

        // Select random room template from list and return
        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];
    }

    /// <summary>
    /// Get unconnected doorways
    /// </summary>
    /// <param name="roomDoorwayList"></param>
    /// <returns>Unconnected, available doorways</returns>
    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
    {
        // Loop through doorway list
        foreach (Doorway doorway in roomDoorwayList)
        {
            if (!doorway.isConnected && !doorway.isUnavailable)
            {
                yield return doorway;
            }
        }
    }

    /// <summary>
    /// Create room based on roomTemplate and layoutMode
    /// </summary>
    /// <param name="roomTemplate"></param>
    /// <param name="roomNode"></param>
    /// <returns>Created room</returns>
    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        // Initialize room from template
        Room room = new Room();

        room.templateID = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;

        // We want a new list with new entries, not just a reference to an existing list
        room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorwayList = CopyDoorwayList(roomTemplate.doorwayList);

        // Set parent ID for room
        if (roomNode.parentRoomNodeIDList.Count == 0) // Entrance
        {
            room.parentRoomID = "";
            room.isPreviouslyVisited = true;
        }
        else
        {
            room.parentRoomID = roomNode.parentRoomNodeIDList[0];
        }

        return room;
    }

    /// <summary>
    /// Select a random room node graph from the list of room node graphs
    /// </summary>
    /// <param name="roomNodeGraphList"></param>
    /// <returns></returns>
    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0)
        {
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            Debug.Log("No room node graphs in list");
            return null;
        }
    }

    /// <summary>
    /// Create deep copy of string list
    /// </summary>
    /// <param name="oldStringList"></param>
    /// <returns>Deep copy of old string list</returns>
    private List<string> CopyStringList(List<string> oldStringList)
    {
        List<string> newStringList = new List<string>();

        foreach (string stringValue in oldStringList)
        {
            newStringList.Add(stringValue);
        }

        return newStringList;
    }

    /// <summary>
    /// Create deep copy of doorway list
    /// </summary>
    /// <param name="oldDoorwayList"></param>
    /// <returns>Deep copy of old doorway list</returns>
    private List<Doorway> CopyDoorwayList(List<Doorway> oldDoorwayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();

        foreach (Doorway doorway in oldDoorwayList)
        {
            Doorway newDoorway = new Doorway();

            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;

            newDoorwayList.Add(newDoorway);
        }

        return newDoorwayList;
    }

    /// <summary>
    /// Instantiate the dungeon room game objects from the prefabs
    /// </summary>
    private void InstantiateRoomGameobjects()
    {
        // Iterate through all dungeon rooms
        foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            // Calculate room positions (remember the room instantiation position needs to be adjusted by the room template lower bounds)
            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, room.lowerBounds.y - room.templateLowerBounds.y, 0f);

            // Instantiate room
            GameObject roomGameObject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);

            // Get instantiated room component from instantiated prefab
            InstantiatedRoom instantiatedRoom = roomGameObject.GetComponentInChildren<InstantiatedRoom>();

            instantiatedRoom.room = room;

            // Initialize the instantiated room
            instantiatedRoom.Initialize(roomGameObject);

            // Save gameobject reference
            room.instantiatedRoom = instantiatedRoom;
        }
    }

    /// <summary>
    /// Get a room by room template ID
    /// </summary>
    /// <param name="roomTemplateID"></param>
    /// <returns>Null if ID does NOT exist</returns>
    private RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        if (roomTemplateDictionary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Get room by roomID
    /// </summary>
    /// <param name="roomID"></param>
    /// <returns>Null if no room exists with provided ID</returns>
    public Room GetRoomByID(string roomID)
    {
        if (dungeonBuilderRoomDictionary.TryGetValue(roomID, out Room room))
        {
            return room;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Clear dungeon room game objects and dungeon room dictionary
    /// </summary>
    private void ClearDungeon()
    {
        // Destroy instantiated dungeon game objects and clear dungeon manager room dictionary
        if (dungeonBuilderRoomDictionary.Count > 0)
        {
            foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
            {
                Room room = keyvaluepair.Value;

                if (room.instantiatedRoom != null)
                {
                    Destroy(room.instantiatedRoom.gameObject);
                }
            }
            dungeonBuilderRoomDictionary.Clear();
        }
    }
}
