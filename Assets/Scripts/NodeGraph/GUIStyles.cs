using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GUIStyles
{
    public GUIStyle entranceNodeStyle = new GUIStyle();
    public GUIStyle entranceNodeSelectedStyle = new GUIStyle();

    public GUIStyle roomNodeStyle = new GUIStyle();
    public GUIStyle roomNodeSelectedStyle = new GUIStyle();

    public GUIStyle bossRoomNodeStyle = new GUIStyle();
    public GUIStyle bossRoomNodeSelectedStyle = new GUIStyle();

    public GUIStyle corridorNodeStyle = new GUIStyle();
    public GUIStyle corridorNodeSelectedStyle = new GUIStyle();


    public GUIStyle chestRoomNodeStyle = new GUIStyle();
    public GUIStyle chestRoomNodeSelectedStyle = new GUIStyle();

    // Node Layout Values
    private const int _NodePadding = 25; // Spacing inside the GUI element
    private const int _NodeBorder = 12; // Spacing outside the GUI element

    /// <summary>
    /// Call all node style setup methods
    /// </summary>
    public void Initialize()
    {
        SetupEntranceNodeStyle();
        SetupRoomNodeStyle();
        SetupBossRoomNodeStyle();
        SetupCorridorNodeStyle();
        SetupChestRoomNodeStyle();
    }

    /// <summary>
    /// Setup the style for the entrance room node
    /// </summary>
    private void SetupEntranceNodeStyle()
    {
        entranceNodeStyle = new GUIStyle();
        entranceNodeStyle.normal.background = EditorGUIUtility.Load("node3") as Texture2D;
        entranceNodeStyle.normal.textColor = Color.white;
        entranceNodeStyle.padding = new RectOffset(_NodePadding, _NodePadding, _NodePadding, _NodePadding);
        entranceNodeStyle.border = new RectOffset(_NodeBorder, _NodeBorder, _NodeBorder, _NodeBorder);

        entranceNodeSelectedStyle = new GUIStyle();
        entranceNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node3 on") as Texture2D;
        entranceNodeSelectedStyle.normal.textColor = Color.white;
        entranceNodeSelectedStyle.padding = entranceNodeStyle.padding;
        entranceNodeSelectedStyle.border = entranceNodeStyle.border;
    }

    /// <summary>
    /// Setup the style for all general room nodes
    /// </summary>
    private void SetupRoomNodeStyle()
    {
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(_NodePadding, _NodePadding, _NodePadding, _NodePadding);
        roomNodeStyle.border = new RectOffset(_NodeBorder, _NodeBorder, _NodeBorder, _NodeBorder);

        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = roomNodeStyle.padding;
        roomNodeSelectedStyle.border = roomNodeStyle.border;
    }

    /// <summary>
    /// Setup the style for the boss room node
    /// </summary>
    private void SetupBossRoomNodeStyle()
    {
        bossRoomNodeStyle = new GUIStyle();
        bossRoomNodeStyle.normal.background = EditorGUIUtility.Load("node6") as Texture2D;
        bossRoomNodeStyle.normal.textColor = Color.black;
        bossRoomNodeStyle.padding = new RectOffset(_NodePadding, _NodePadding, _NodePadding, _NodePadding);
        bossRoomNodeStyle.border = new RectOffset(_NodeBorder, _NodeBorder, _NodeBorder, _NodeBorder);

        bossRoomNodeSelectedStyle = new GUIStyle();
        bossRoomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node6 on") as Texture2D;
        bossRoomNodeSelectedStyle.normal.textColor = Color.black;
        bossRoomNodeSelectedStyle.padding = bossRoomNodeStyle.padding;
        bossRoomNodeSelectedStyle.border = bossRoomNodeStyle.border;
    }

    /// <summary>
    /// Setup the style for the corridor nodes
    /// </summary>
    private void SetupCorridorNodeStyle()
    {
        corridorNodeStyle = new GUIStyle();
        corridorNodeStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;
        corridorNodeStyle.normal.textColor = Color.white;
        corridorNodeStyle.padding = new RectOffset(_NodePadding, _NodePadding, _NodePadding, _NodePadding);
        corridorNodeStyle.border = new RectOffset(_NodeBorder, _NodeBorder, _NodeBorder, _NodeBorder);

        corridorNodeSelectedStyle = new GUIStyle();
        corridorNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node0 on") as Texture2D;
        corridorNodeSelectedStyle.normal.textColor = Color.white;
        corridorNodeSelectedStyle.padding = corridorNodeStyle.padding;
        corridorNodeSelectedStyle.border = corridorNodeStyle.border;
    }

    /// <summary>
    /// Setup the style for all the chest room nodes
    /// </summary>
    private void SetupChestRoomNodeStyle()
    {
        chestRoomNodeStyle = new GUIStyle();
        chestRoomNodeStyle.normal.background = EditorGUIUtility.Load("node4") as Texture2D;
        chestRoomNodeStyle.normal.textColor = Color.black;
        chestRoomNodeStyle.padding = new RectOffset(_NodePadding, _NodePadding, _NodePadding, _NodePadding);
        chestRoomNodeStyle.border = new RectOffset(_NodeBorder, _NodeBorder, _NodeBorder, _NodeBorder);

        chestRoomNodeSelectedStyle = new GUIStyle();
        chestRoomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node4 on") as Texture2D;
        chestRoomNodeSelectedStyle.normal.textColor = Color.black;
        chestRoomNodeSelectedStyle.padding = chestRoomNodeStyle.padding;
        chestRoomNodeSelectedStyle.border = chestRoomNodeStyle.border;
    }
}
