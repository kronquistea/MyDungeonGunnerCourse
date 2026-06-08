using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Audio;

public class GameResources : MonoBehaviour
{
    private static GameResources instance;

    public static GameResources Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<GameResources>("GameResources");
            }
            return instance;
        }
    }

    #region Header DUNGEON
    [Space(10)]
    [Header("Dungeon")]
    #endregion

    #region Tooltip
    [Tooltip("Populate with the dungeon RoomNodeTypeListSO")]
    #endregion
    public RoomNodeTypeListSO roomNodeTypeList;

    #region Header PLAYER SELECTION
    [Space(10)]
    [Header("PLAYER SELECTION")]
    #endregion

    #region Tooltip
    [Tooltip("Populate with the player selection prefab")]
    #endregion
    public GameObject playerSelectionPrefab;

    #region Header PLAYER
    [Space(10)]
    [Header("PLAYER")]
    #endregion

    #region Tooltip
    [Tooltip("The current player scriptable object - this is used to reference the current player between scenes")]
    #endregion
    public CurrentPlayerSO currentPlayer;

    #region Tooltip
    [Tooltip("Player details list - populate list with playerdetails SOs")]
    #endregion
    public List<PlayerDetailsSO> playerDetailsList;

    #region Header MUSIC
    [Space(10)]
    [Header("MUSIC")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the music master mixer group")]
    #endregion
    public AudioMixerGroup musicMasterMixerGroup;

    #region Tooltip
    [Tooltip("Main menu music SO")]
    #endregion
    public MusicTrackSO mainMenuMusic;

    #region Tooltip
    [Tooltip("Music on full snapshot")]
    #endregion
    public AudioMixerSnapshot musicOnFullSnapshot;

    #region Tooltip
    [Tooltip("Music low snapshot")]
    #endregion
    public AudioMixerSnapshot musicLowSnapshot;

    #region Tooltip
    [Tooltip("Music off snapshot")]
    #endregion
    public AudioMixerSnapshot musicOffSnapshot;

    #region Header SOUNDS
    [Space(10)]
    [Header("SOUNDS")]
    #endregion

    #region Tooltip
    [Tooltip("Populate with the sounds master mixer group")]
    #endregion
    public AudioMixerGroup soundsMasterMixerGroup;

    #region Tooltip
    [Tooltip("Door open close sound effect")]
    #endregion
    public SoundEffectSO doorOpenCloseSoundEffect;

    #region Tooltip
    [Tooltip("Populate with the table flip sound effect")]
    #endregion
    public SoundEffectSO tableFlip;

    #region Tooltip
    [Tooltip("Populate with the chest open sound effect")]
    #endregion
    public SoundEffectSO chestOpen;

    #region Tooltip
    [Tooltip("Populate with the health pickup sound effect")]
    #endregion
    public SoundEffectSO healthPickup;

    #region Tooltip
    [Tooltip("Populate with the weapon pickup sound effect")]
    #endregion
    public SoundEffectSO weaponPickup;

    #region Tooltip
    [Tooltip("Populate with the ammo pickup sound effect")]
    #endregion
    public SoundEffectSO ammoPickup;

    #region Header MATERIALS
    [Space(10)]
    [Header("Materials")]
    #endregion

    #region Tooltip
    [Tooltip("Dimmed Material")]
    #endregion
    public Material dimmedMaterial;

    #region Tooltip
    [Tooltip("Sprite-Lit-Default Material")]
    #endregion
    public Material litMaterial;

    #region Tooltip
    [Tooltip("Populate with the Variable Lit Shader")]
    #endregion
    public Shader variableLitShader;

    #region Tooltip
    [Tooltip("Populate with the materialize shader")]
    #endregion
    public Shader materializeShader;

    #region Tooltip
    [Tooltip("Populate with FlameShaderMaterial")]
    #endregion
    public Material flameShaderMaterialZero;

    #region Tooltip
    [Tooltip("Populate with FlameShaderMaterial1")]
    #endregion
    public Material flameShaderMaterialOne;

    #region Tooltip
    [Tooltip("Populate with FlameShaderMaterial2")]
    #endregion
    public Material flameShaderMaterialTwo;

    #region Header SPECIAL TILEMAP TILES
    [Space(10)]
    [Header("SPECIAL TILEMAP TILES")]
    #endregion

    #region Tooltip
    [Tooltip("Collision tiles that the enemies can navigate to")]
    #endregion
    public TileBase[] enemyUnwalkableCollisionTilesArray;

    #region Tooltip
    [Tooltip("Preferred path tile for enemy navigation")]
    #endregion
    public TileBase preferredEnemyTilePath;

    #region Header UI
    [Space(10)]
    [Header("UI")]
    #endregion

    #region Tooltip
    [Tooltip("Populate with heart icon prefab")]
    #endregion
    public GameObject heartIconPrefab;
    
    #region Tooltip
    [Tooltip("Populate with ammo icon prefab")]
    #endregion
    public GameObject ammoIconPrefab;

    #region Tooltip
    [Tooltip("Populate with the score prefab")]
    #endregion
    public GameObject scorePrefab;

    #region Header CHESTS
    [Space(10)]
    [Header("CHEST")]
    #endregion

    #region Tooltip
    [Tooltip("Chest item prefab")]
    #endregion
    public GameObject chestItemPrefab;

    #region Tooltip
    [Tooltip("Populate with heart icon sprite")]
    #endregion
    public Sprite heartIcon;

    #region Tooltip
    [Tooltip("Populate with bullet icon sprite")]
    #endregion
    public Sprite bulletIcon;

    #region Header MINIMAP
    [Space(10)]
    [Header("MINIMAP")]
    #endregion

    #region Tooltip
    [Tooltip("Minimap skull prefab")]
    #endregion
    public GameObject minimapSkullPrefab;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        //DUNGEON
        HelperUtilities.ValidateCheckNullValue(this, nameof(roomNodeTypeList), roomNodeTypeList);

        // PLAYER SELECTION
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerSelectionPrefab), playerSelectionPrefab);

        // PLAYER
        HelperUtilities.ValidateCheckNullValue(this, nameof(currentPlayer), currentPlayer);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(playerDetailsList), playerDetailsList);

        // MUSIC
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicMasterMixerGroup), musicMasterMixerGroup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(mainMenuMusic), mainMenuMusic);
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicOnFullSnapshot), musicOnFullSnapshot);
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicLowSnapshot), musicLowSnapshot);
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicOffSnapshot), musicOffSnapshot);

        // SOUNDS
        HelperUtilities.ValidateCheckNullValue(this, nameof(soundsMasterMixerGroup), soundsMasterMixerGroup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(doorOpenCloseSoundEffect), doorOpenCloseSoundEffect);
        HelperUtilities.ValidateCheckNullValue(this, nameof(tableFlip), tableFlip);
        HelperUtilities.ValidateCheckNullValue(this, nameof(chestOpen), chestOpen);
        HelperUtilities.ValidateCheckNullValue(this, nameof(healthPickup), healthPickup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponPickup), weaponPickup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoPickup), ammoPickup);

        // MATERIALS
        HelperUtilities.ValidateCheckNullValue(this, nameof(dimmedMaterial), dimmedMaterial);
        HelperUtilities.ValidateCheckNullValue(this, nameof(litMaterial), litMaterial);
        HelperUtilities.ValidateCheckNullValue(this, nameof(variableLitShader), variableLitShader);
        HelperUtilities.ValidateCheckNullValue(this, nameof(materializeShader), materializeShader);
        HelperUtilities.ValidateCheckNullValue(this, nameof(flameShaderMaterialZero), flameShaderMaterialZero);
        HelperUtilities.ValidateCheckNullValue(this, nameof(flameShaderMaterialOne), flameShaderMaterialOne);
        HelperUtilities.ValidateCheckNullValue(this, nameof(flameShaderMaterialTwo), flameShaderMaterialTwo);

        // SPECIAL TILEMAP TILES
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(enemyUnwalkableCollisionTilesArray), enemyUnwalkableCollisionTilesArray);
        HelperUtilities.ValidateCheckNullValue(this, nameof(preferredEnemyTilePath), preferredEnemyTilePath);

        // UI
        HelperUtilities.ValidateCheckNullValue(this, nameof(heartIconPrefab), heartIconPrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoIconPrefab), ammoIconPrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(scorePrefab), scorePrefab);

        // CHESTS
        HelperUtilities.ValidateCheckNullValue(this, nameof(chestItemPrefab), chestItemPrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(heartIcon), heartIcon);
        HelperUtilities.ValidateCheckNullValue(this, nameof(bulletIcon), bulletIcon);

        // MINIMAP
        HelperUtilities.ValidateCheckNullValue(this, nameof(minimapSkullPrefab), minimapSkullPrefab);
    }
#endif
    #endregion
}
