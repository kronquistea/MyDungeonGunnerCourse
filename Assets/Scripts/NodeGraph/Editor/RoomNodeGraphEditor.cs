using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyles _styles = new GUIStyles();

    private static RoomNodeGraphSO _currentRoomNodeGraph;
    
    private RoomNodeSO _currentRoomNode = null;
    private RoomNodeTypeListSO _roomNodeTypeList;

    private Vector2 _graphOffset;
    private Vector2 _graphDrag;


    // Node layout values
    private const float _RoomNodeWidth = 160f;
    private const float _RoomNodeHeight = 75f;

    // Connecting line values
    private const float _ConnectingLineWidth = 3f;
    private const float _ConnectingLineArrowSize = 6f;

    // Grid Spacing
    private const float _GridLarge = 100f;
    private const float _GridSmall = 25f;

    [MenuItem("Room Node Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]

    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable()
    {
        // Subscribe to the inspector selection changed event
        Selection.selectionChanged += InspectorSelectionChanged;

        _styles.Initialize();

        // Load room node types
        _roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        // Unsubscribe from the inspector selection changed event
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    /// <summary>
    /// Open the room node graph editor window if a room node graph SO asset is double clicked in the inspector
    /// </summary>
    /// <param name="instanceID"></param>
    /// <param name="line"></param>
    /// <returns>Something, don't know yet</returns>
    [OnOpenAsset(0)] // Need the namespace UnityEditor.Callbacks
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;

        // Check if a room node graph SO was clicked on in the editor
        if (roomNodeGraph != null)
        {
            OpenWindow();

            _currentRoomNodeGraph = roomNodeGraph;

            return true;
        }
        return false;
    }

    /// <summary>
    /// Draw Editor GUI
    /// </summary>
    private void OnGUI()
    {
        // If a scriptable object of type RoomNodeGraphSO has been selected then process
        if (_currentRoomNodeGraph != null)
        {
            // Draw grid
            DrawBackgroundGrid(_GridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(_GridLarge, 0.3f, Color.gray);

            // Draw line if being dragged
            DrawDraggedLine();

            // Process Events (mouse clicks, selecting stuff in the editor, etc)
            ProcessEvents(Event.current);

            // Draw connections between room nodes
            DrawRoomNodeConnections();

            // Draw Room Nodes (draws the room nodes we have created to the screen)
            DrawRoomNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }

    /// <summary>
    /// Draw a background grid for the room node graph editor
    /// </summary>
    /// <param name="gridSize"></param>
    /// <param name="gridOpacity"></param>
    /// <param name="gridColor"></param>
    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        _graphOffset += _graphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(_graphOffset.x % gridSize, _graphOffset.y % gridSize, 0);

        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(
                new Vector3(gridSize * i, -gridSize, 0) + gridOffset,
                new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);
        }

        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(
                new Vector3(-gridSize, gridSize * j, 0) + gridOffset,
                new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset);
        }

        Handles.color = Color.white;
    }

    /// <summary>
    /// Draw the dragged line
    /// </summary>
    private void DrawDraggedLine()
    {
        if (_currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            // Draw line from node to line position
            Handles.DrawBezier(
                _currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center,
                _currentRoomNodeGraph.linePosition,
                _currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center,
                _currentRoomNodeGraph.linePosition,
                Color.white,
                null,
                _ConnectingLineWidth);
        }
    }

    /// <summary>
    /// Process event
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessEvents(Event currentEvent)
    {
        // Reset graph drag
        _graphDrag = Vector2.zero;

        // Get room node that mouse is over if it's null or not currently begin dragged
        if (_currentRoomNode == null || _currentRoomNode.isLeftClickDragging == false)
        {
            _currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        // If the mouse isn't over a room node, or we are currently dragging a line from the room node then process graph events
        if (_currentRoomNode == null || _currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        // Else process room node events
        else
        {
            // Process room node events
            _currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    /// <summary>
    /// Check to see if the mouse is over a room node - if so then return the room node else return null
    /// </summary>
    /// <param name="currentEvent"></param>
    /// <returns>RoomNodeSO (room node) if mouse is over a room node - null otherwise</returns>
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for (int i = _currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            if (_currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return _currentRoomNodeGraph.roomNodeList[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Process room node graph events
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            // Process mouse down events
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            // Process mouse down events
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            // Process mouse drag event
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Process mouse down events on the room node graph (not over a node)
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // Process right click mouse down on graph event (show context menu) (0 = LMB, 1 = RMB)
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        // Process left mouse down on graph event
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    /// <summary>
    /// Show the context menu
    /// </summary>
    /// <param name="mousePosition"></param>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeleteSelectedRoomNodes);

        menu.ShowAsContext();
    }

    /// <summary>
    /// Create a room node at the mouse position
    /// </summary>
    /// <param name="mousePositionObject"></param>
    private void CreateRoomNode(object mousePositionObject)
    {
        // If currrent node graph is empty then add entrance room node first
        if (_currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200f, 200f), _roomNodeTypeList.list.Find(x => x.isEntrance));
        }

        CreateRoomNode(mousePositionObject, _roomNodeTypeList.list.Find(x => x.isNone));
    }

    /// <summary>
    /// Create a room node at the mouse position - overloaded to also pass in RoomNodeType
    /// </summary>
    /// <param name="mousePositionObject"></param>
    /// <param name="roomNodeType"></param>
    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // Create room node scriptable object asset
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        // Add room node to current room node graph room node list
        _currentRoomNodeGraph.roomNodeList.Add(roomNode);

        // Set room node values
        roomNode.Initialize(new Rect(mousePosition, new Vector2(_RoomNodeWidth, _RoomNodeHeight)), _currentRoomNodeGraph, roomNodeType);

        // Add room node to room node graph scriptable object asset database
        AssetDatabase.AddObjectToAsset(roomNode, _currentRoomNodeGraph);

        AssetDatabase.SaveAssets();

        // Refresh graph node dictionary
        _currentRoomNodeGraph.OnValidate();
    }

    /// <summary>
    /// Delete selected room nodes
    /// </summary>
    private void DeleteSelectedRoomNodes()
    {
        Queue<RoomNodeSO> roomNodeDeletionQueue = new Queue<RoomNodeSO>();

        // Loop through all nodes
        foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomNodeDeletionQueue.Enqueue(roomNode);

                // Iterate through child room node ids
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // Retrieve child room node
                    RoomNodeSO childRoomNode = _currentRoomNodeGraph.GetRoomNode(childRoomNodeID);

                    if (childRoomNode != null)
                    {
                        // Remove parentID from child room node
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                // Iterate through parent room node ids
                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    // Retrieve parent node
                    RoomNodeSO parentRoomNode = _currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);

                    if (parentRoomNode != null)
                    {
                        // Remove childID from parent node
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }

        }

        // Delete queued room nodes
        while (roomNodeDeletionQueue.Count > 0)
        {
            // Get room node from queue
            RoomNodeSO roomNodeToDelete = roomNodeDeletionQueue.Dequeue();

            // Remove node from dictionary
            _currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);

            // Remove node from list
            _currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);

            // Remove node from Asset database
            DestroyImmediate(roomNodeToDelete, true);

            // Save Asset database
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// Delete the links between the selected room nodes
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
        // Iterate through all room nodes
        foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    // Get child room node
                    RoomNodeSO childRoomNode = _currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    // If the child room node is selected
                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        // Remove childID from parent room node
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                        // Remove parentId from child room node
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        ClearAllSelectedRoomNodes();
    }

    /// <summary>
    /// Clear selection from all room nodes
    /// </summary>
    private void ClearAllSelectedRoomNodes()
    {
        foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;

                GUI.changed = true;
            }
        }
    }

    /// <summary>
    /// Select all room nodes
    /// </summary>
    private void SelectAllRoomNodes()
    {
        foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }

    /// <summary>
    /// Process mouse up event
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // If releasing RMB and currently dragging a line
        if (currentEvent.button == 1 && _currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            // Check to see if hovering over a room node
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

            if (roomNode != null)
            {
                // If so, set it as a child of the parent room node if it can be added
                if (_currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    // Set parent ID in child room node
                    roomNode.AddParentRoomNodeIDToRoomNode(_currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }
            ClearLineDrag();
        }
    }

    /// <summary>
    /// Process mouse drag event
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // Process right click drag event - draw line
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
        // Process left click drage event - drag node graph
        else if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    /// <summary>
    /// Process right mouse drag event - draw line
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (_currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    /// <summary>
    /// Process left mouse drag event - drag room node graph
    /// </summary>
    /// <param name="dragDelta"></param>
    private void ProcessLeftMouseDragEvent(Vector2 dragDelta)
    {
        _graphDrag = dragDelta;

        for (int i = 0; i < _currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            _currentRoomNodeGraph.roomNodeList[i].DragNode(dragDelta);
        }

        GUI.changed = true;
    }

    /// <summary>
    /// Drag connecting line from room node
    /// </summary>
    /// <param name="delta"></param>
    public void DragConnectingLine(Vector2 delta)
    {
        _currentRoomNodeGraph.linePosition += delta;
    }

    /// <summary>
    /// Clear line drag from a room node
    /// </summary>
    private void ClearLineDrag()
    {
        _currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        _currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    /// <summary>
    /// Draw connections in the graph window between room nodes
    /// </summary>
    private void DrawRoomNodeConnections()
    {
        // Loop through all room nodes
        foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.roomNodeList)
        {
            // If the room node has any children
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                // Loop through child room nodes
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // Get child room from dictionary
                    // I don't understand why this if statement is necessary - can't we just execute the following code just as well without it?
                    if (_currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, _currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);

                        GUI.changed = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Draw connection lines between parent room node and child room node
    /// </summary>
    /// <param name="parentRoomNode"></param>
    /// <param name="childRoomNode"></param>
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        // Get line start and end positions
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        // Calculate midway point
        Vector2 midPosition = (endPosition + startPosition) / 2f;

        // Vector from start to end position of line
        Vector2 direction = endPosition - startPosition;

        // Calculate normalized perpendicular positions from the mid points
        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * _ConnectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * _ConnectingLineArrowSize;

        // Calculate midpoint offset position for arrow head
        Vector2 arrowHeadPoint = midPosition + direction.normalized * _ConnectingLineArrowSize;

        // Draw arrow
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, _ConnectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, _ConnectingLineWidth);

        // Draw line
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, _ConnectingLineWidth);

        GUI.changed = true;
    }

    /// <summary>
    /// Draw room nodes in the graph window
    /// </summary>
    private void DrawRoomNodes()
    {
        // Loop through all room nodes and draw them
        foreach (RoomNodeSO roomNode in _currentRoomNodeGraph.roomNodeList)
        {
            roomNode.Draw(GetRoomNodeStyle(roomNode));
        }
        GUI.changed = true;
    }

    private GUIStyle GetRoomNodeStyle(RoomNodeSO roomNode)
    {
        if (roomNode.roomNodeType.isEntrance)
        {
            return roomNode.isSelected ? _styles.entranceNodeSelectedStyle : _styles.entranceNodeStyle;
        }
        if (roomNode.roomNodeType.isCorridor)
        {
            return roomNode.isSelected ? _styles.corridorNodeSelectedStyle : _styles.corridorNodeStyle;
        }
        if (roomNode.roomNodeType.isBossRoom)
        {
            return roomNode.isSelected ? _styles.bossRoomNodeSelectedStyle : _styles.bossRoomNodeStyle;
        }
        if (roomNode.roomNodeType.isChestRoom)
        {
            return roomNode.isSelected ? _styles.chestRoomNodeSelectedStyle : _styles.chestRoomNodeStyle;
        }
        return roomNode.isSelected ? _styles.roomNodeSelectedStyle : _styles.roomNodeStyle;
    }

    /// <summary>
    /// Selection changed in the inspector
    /// </summary>
    private void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            _currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }
}
