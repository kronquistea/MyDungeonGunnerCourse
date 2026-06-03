using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(MaterializeEffect))]

public class Chest : MonoBehaviour, IUseable
{
    #region Tooltip
    [Tooltip("Set this to the color to be used for the materialization effect")]
    #endregion
    [ColorUsage(false, true)]
    [SerializeField] private Color materializeColor;

    #region Tooltip
    [Tooltip("Set this to the time it will take to materialize the chest")]
    #endregion
    [SerializeField] private float materializeTime = 3f;

    #region Tooltip
    [Tooltip("Populate with itemSpawnPoint transform")]
    #endregion
    [SerializeField] private Transform itemSpawnPoint;

    private int healthPercent;
    private int ammoPercent;
    private WeaponDetailsSO weaponDetails;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private MaterializeEffect materializeEffect;
    private bool isEnabled = false;
    private ChestState chestState = ChestState.closed;
    private GameObject chestItemGameObject;
    private ChestItem chestItem;
    private TextMeshPro messageTextTMP;

    private void Awake()
    {
        // Load components
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        materializeEffect = GetComponent<MaterializeEffect>();
        messageTextTMP = GetComponent<TextMeshPro>();
    }

    /// <summary>
    /// Initialize chest and either make it visible immediately or materialize it
    /// </summary>
    /// <param name="shouldMaterialize"></param>
    /// <param name="healthPercent"></param>
    /// <param name="ammoPercent"></param>
    /// <param name="weaponDetails"></param>
    public void Initialize(bool shouldMaterialize, int healthPercent, int ammoPercent, WeaponDetailsSO weaponDetails)
    {
        this.healthPercent = healthPercent;
        this.ammoPercent = ammoPercent;
        this.weaponDetails = weaponDetails;

        // Check if the chest should be materialized
        if (shouldMaterialize)
        {
            // Start materializing the chest
            StartCoroutine(MaterializeChest());
        }
        else
        {
            // Simply enable the chest
            EnableChest();
        }
    }

    private IEnumerator MaterializeChest()
    {
        // Sprite renderer array to be used to materialize the chest (only has one item in it)
        SpriteRenderer[] spriteRendererArray = new SpriteRenderer[] { spriteRenderer };

        // Wait until the chest is fully materialized before continuing
        yield return StartCoroutine(materializeEffect.MaterializeRoutine(GameResources.Instance.materializeShader, materializeColor, materializeTime, spriteRendererArray, GameResources.Instance.litMaterial));

        // Enable the chest after it has been materialized
        EnableChest();
    }

    /// <summary>
    /// Enable the chest
    /// </summary>
    private void EnableChest()
    {
        isEnabled = true;
    }

    /// <summary>
    /// Use the chest - implemented due to IUseable interface
    /// </summary>
    public void UseItem()
    {
        // Check if the chest is not yet enabled
        if (!isEnabled)
        {
            // Do nothing (chest is not enabled)
            return;
        }

        switch (chestState)
        {
            case ChestState.closed:
                OpenChest();
                break;

            case ChestState.healthItem:
                CollectHealthItem();
                break;

            case ChestState.ammoItem:
                CollectAmmoItem();
                break;

            case ChestState.weaponItem:
                CollectWeaponItem();
                break;

            case ChestState.empty:
                return;
            
            default:
                return;
        }
    }

    /// <summary>
    /// Open the chest on first use
    /// </summary>
    private void OpenChest()
    {
        // Animator chest open
        animator.SetBool(Settings.use, true);

        // Play chest open sound effect
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.chestOpen);

        // Check if the chest contains a weapon item
        if (weaponDetails != null)
        {
            // Check if the weapon item in the chest is already held by the player
            if (GameManager.Instance.GetPlayer().IsWeaponHeldByPlayer(weaponDetails))
            {
                // Player already has the weapon - so set the weapon item to null and do not display it to the player
                weaponDetails = null;
            }
        }

        // Update the chest state to the next state
        UpdateChestState();
    }

    private void UpdateChestState()
    {
        // Check if the chest contains a health item
        if (healthPercent != 0)
        {
            chestState = ChestState.healthItem;
            InstantiateHealthItem();
        }
        // Else check if the chest contains an ammo item
        else if (ammoPercent != 0)
        {
            chestState = ChestState.ammoItem;
            InstantiateAmmoItem();
        }
        // Else check if the chest contains a weapon item
        else if (weaponDetails != null)
        {
            chestState = ChestState.weaponItem;
            InstantiateWeaponItem();
        }
        // Else set the chest state to empty
        else
        {
            chestState = ChestState.empty;
        }
    }

    /// <summary>
    /// Instantiate a new chest item
    /// </summary>
    private void InstantiateItem()
    {
        // Instantiate the chest item
        chestItemGameObject = Instantiate(GameResources.Instance.chestItemPrefab, this.transform);

        // Set the chest item
        chestItem = chestItemGameObject.GetComponent<ChestItem>();
    }

    /// <summary>
    /// Instantiate a health item
    /// </summary>
    private void InstantiateHealthItem()
    {
        // Instantiate a new chest item to be used
        InstantiateItem();

        // Initialize the chest item as a health item specifically
        chestItem.Initialize(GameResources.Instance.heartIcon, healthPercent.ToString() + "%", itemSpawnPoint.position, materializeColor);
    }

    /// <summary>
    /// Collect the health item and add it to the health component for the player
    /// </summary>
    private void CollectHealthItem()
    {
        // Check if there is no chest item or the chest item has not yet been materialized
        if (chestItem == null || !chestItem.isItemMaterialized)
        {
            // Do nothing (there is nothing to anything with!)
            return;
        }

        // Add the health to the player
        GameManager.Instance.GetPlayer().health.AddHealth(healthPercent);

        // Play the sound effect of picking up health
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.healthPickup);

        // Reset health percent (as the item was used)
        healthPercent = 0;

        // Destroy the chest item game object to clear memory
        Destroy(chestItemGameObject);

        // Update the chest state as the health item has been received by the player
        UpdateChestState();
    }

    /// <summary>
    /// Instantiate a ammo item for the player to collect
    /// </summary>
    private void InstantiateAmmoItem()
    {
        // Instantiate a new chest item to be used
        InstantiateItem();

        // Initialize the chest item as an ammo item specifically
        chestItem.Initialize(GameResources.Instance.bulletIcon, ammoPercent.ToString() + "%", itemSpawnPoint.position, materializeColor);
    }

    /// <summary>
    /// Collect the ammo item and add it to the ammo in the player's current weapon
    /// </summary>
    private void CollectAmmoItem()
    {
        // Check if there is no chest item or the chest item has not yet been materialized
        if (chestItem == null || !chestItem.isItemMaterialized)
        {
            // Do nothing (there is nothing to anything with!)
            return;
        }

        // Get the player instance
        Player player = GameManager.Instance.GetPlayer();

        // Reload the player's active weapon
        player.reloadWeaponEvent.CallReloadWeaponEvent(player.activeWeapon.GetCurrentWeapon(), ammoPercent);

        // Play the ammo pickup sound effect
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);

        // Reset ammo percent (as it was just picked up)
        ammoPercent = 0;

        // Destroy the item to cleanup memory (performance)
        Destroy(chestItemGameObject);

        // Update the chest state to the next state as the ammo was just picked up
        UpdateChestState();
    }

    /// <summary>
    /// Instantiate a weapon item for the player to collect
    /// </summary>
    private void InstantiateWeaponItem()
    {
        // Instantiate a chest item
        InstantiateItem();

        // Initialize a weapon game object specifically
        chestItem.Initialize(weaponDetails.weaponSprite, weaponDetails.weaponName, itemSpawnPoint.position, materializeColor);
    }

    private void CollectWeaponItem()
    {
        // Check if there is no chest item or the chest item has not yet been materialized
        if (chestItem == null || !chestItem.isItemMaterialized)
        {
            // Do nothing (there is nothing to anything with!)
            return;
        }

        // Check if the player is not already holding the weapon in the chest
        if (!GameManager.Instance.GetPlayer().IsWeaponHeldByPlayer(weaponDetails))
        {
            // Add the weapon to the player
            GameManager.Instance.GetPlayer().AddWeaponToPlayer(weaponDetails);

            // Play weapon pickup sound effect
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);
        }
        else
        {
            // Display weapon already equipped message
            StartCoroutine(DisplayMessage("WEAPON\nALREADY\nEQUIPPED", 5f));
        }

        // Reset weapon details as weapon was just picked uo
        weaponDetails = null;

        // Clear up memory (performance)
        Destroy(chestItemGameObject);

        // Update chest state as weapon was just picked up
        UpdateChestState();
    }

    /// <summary>
    /// Display passed in message
    /// </summary>
    /// <param name="text"></param>
    /// <returns>Coroutine</returns>
    private IEnumerator DisplayMessage(string messageText, float messageDisplayTime)
    {
        // Set message text
        messageTextTMP.text = messageText;

        // Wait 5 seconds (before removing text)
        yield return new WaitForSeconds(messageDisplayTime);

        // Remove text
        messageTextTMP.text = "";
    }
}
