using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponStatusUI : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with image component on the child WeaponImage gameobject")]
    #endregion
    [SerializeField] private Image weaponImage;

    #region Tooltip
    [Tooltip("Populate wtih the Transform from the child AmmoHolder gameobject")]
    #endregion
    [SerializeField] private Transform ammoHolderTransform;

    #region Tooltip
    [Tooltip("Populate with the TextMeshPro-Text component on the child ReloadText gameobject")]
    #endregion
    [SerializeField] private TextMeshProUGUI reloadText;

    #region Tooltip
    [Tooltip("Populate with the TextMeshPro-Text component on the child AmmoRemainingText gameobject")]
    #endregion
    [SerializeField] private TextMeshProUGUI ammoRemainingText;

    #region Tooltip
    [Tooltip("Populate with the TextMeshPro-Text component on the child WeaponNameText gameobject")]
    #endregion
    [SerializeField] private TextMeshProUGUI weaponNameText;

    #region Tooltip
    [Tooltip("Populate with the RectTransform of the child gameobject ReloadBar")]
    #endregion
    [SerializeField] private Transform reloadBar;

    #region Tooltip
    [Tooltip("Populate with the Image component of the child gameobject BarImage")]
    #endregion
    [SerializeField] private Image barImage;

    private Player player;
    private List<GameObject> ammoIconList = new List<GameObject>();
    private Coroutine reloadWeaponCoroutine;
    private Coroutine blinkingReloadTextCoroutine;
    private Coroutine blinkingOutOfAmmoTextCoroutine;

    private void Awake()
    {
        // Get player
        player = GameManager.Instance.GetPlayer();
    }

    private void OnEnable()
    {
        // Subscribe to set active weapon event
        player.setActiveWeaponEvent.OnSetActiveWeapon += SetActiveWeaponEvent_OnSetActiveWeapon;

        // Subscribe to weapon fired event
        player.weaponFiredEvent.OnWeaponFired += WeaponFiredEvent_OnWeaponFired;

        // Subscribe to reload weapon event
        player.reloadWeaponEvent.OnReloadWeapon += ReloadWeaponEvent_OnReloadWeapon;

        // Subscribe to weapon reloaded event
        player.weaponReloadedEvent.OnWeaponReloaded += WeaponReloadedEvent_OnWeaponReloaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from set active weapon event
        player.setActiveWeaponEvent.OnSetActiveWeapon -= SetActiveWeaponEvent_OnSetActiveWeapon;

        // Unsubscribe from weapon fired event
        player.weaponFiredEvent.OnWeaponFired -= WeaponFiredEvent_OnWeaponFired;

        // Unsubscribe from reload weapon event
        player.reloadWeaponEvent.OnReloadWeapon -= ReloadWeaponEvent_OnReloadWeapon;

        // Unsubscribe from weapon reloaded event
        player.weaponReloadedEvent.OnWeaponReloaded -= WeaponReloadedEvent_OnWeaponReloaded;
    }

    private void Start()
    {
        // Update active weapon status on the UI
        SetActiveWeapon(player.activeWeapon.GetCurrentWeapon());
    }

    /// <summary>
    /// Handle set active weapon event on the UI
    /// </summary>
    /// <param name="setActiveWeaponEvent"></param>
    /// <param name="setActiveWeaponEventArgs"></param>
    private void SetActiveWeaponEvent_OnSetActiveWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetActiveWeapon(setActiveWeaponEventArgs.weapon);
    }

    /// <summary>
    /// Handle weapon fired event on the UI
    /// </summary>
    /// <param name="weaponFiredEvent"></param>
    /// <param name="weaponFiredEventArgs"></param>
    private void WeaponFiredEvent_OnWeaponFired(WeaponFiredEvent weaponFiredEvent, WeaponFiredEventArgs weaponFiredEventArgs)
    {
        WeaponFired(weaponFiredEventArgs.weapon);
    }

    /// <summary>
    /// Weapon fired update UI
    /// </summary>
    /// <param name="weapon"></param>
    private void WeaponFired(Weapon weapon)
    {
        UpdateAmmoText(weapon);
        UpdateAmmoLoadedIcons(weapon);
        UpdateReloadText(weapon);
    }

    /// <summary>
    /// Handle reload weapon event on the UI
    /// </summary>
    /// <param name="reloadWeaponEvent"></param>
    /// <param name="reloadWeaponEventArgs"></param>
    private void ReloadWeaponEvent_OnReloadWeapon(ReloadWeaponEvent reloadWeaponEvent ,ReloadWeaponEventArgs reloadWeaponEventArgs)
    {
        UpdateWeaponReloadBar(reloadWeaponEventArgs.weapon);
    }

    /// <summary>
    /// Handle weapon reloaded event on the UI
    /// </summary>
    /// <param name="weaponReloadedEvent"></param>
    /// <param name="weaponReloadedEventArgs"></param>
    private void WeaponReloadedEvent_OnWeaponReloaded(WeaponReloadedEvent weaponReloadedEvent, WeaponReloadedEventArgs weaponReloadedEventArgs)
    {
        WeaponReloaded(weaponReloadedEventArgs.weapon);
    }

    /// <summary>
    /// Weapon has been reloaded - update UI if current weapon
    /// </summary>
    /// <param name="weapon"></param>
    private void WeaponReloaded(Weapon weapon)
    {
        // If weapon reloaded is the current weapon
        if (player.activeWeapon.GetCurrentWeapon() == weapon)
        {
            UpdateReloadText(weapon);
            UpdateAmmoText(weapon);
            UpdateAmmoLoadedIcons(weapon);
            ResetWeaponReloadBar();
        }
    }

    /// <summary>
    /// Set the active weapon on the UI
    /// </summary>
    /// <param name="weapon"></param>
    private void SetActiveWeapon(Weapon weapon)
    {
        UpdateActiveWeaponImage(weapon.weaponDetails);
        UpdateActiveWeaponName(weapon);
        UpdateAmmoText(weapon);
        UpdateAmmoLoadedIcons(weapon);

        // If set weapon is still reloading then update reload bar
        if (weapon.isWeaponReloading)
        {
            UpdateWeaponReloadBar(weapon);
        }
        else
        {
            ResetWeaponReloadBar();
        }

        UpdateReloadText(weapon);
    }

    /// <summary>
    /// Populate active weapon image
    /// </summary>
    /// <param name="weaponDetails"></param>
    private void UpdateActiveWeaponImage(WeaponDetailsSO weaponDetails)
    {
        weaponImage.sprite = weaponDetails.weaponSprite;
    }

    /// <summary>
    /// Populate active weapon name
    /// </summary>
    /// <param name="weapon"></param>
    private void UpdateActiveWeaponName(Weapon weapon)
    {
        weaponNameText.text = "(" + weapon.weaponListPosition + ") " + weapon.weaponDetails.weaponName.ToUpper();
    }

    /// <summary>
    /// Update the ammo remaining text on the UI
    /// </summary>
    /// <param name="weapon"></param>
    private void UpdateAmmoText(Weapon weapon)
    {
        // Display infinite ammo if weapon has infinite ammo
        if (weapon.weaponDetails.hasInfiniteAmmo)
        {
            ammoRemainingText.text = "INFINITE AMMO";
        }
        // Otherwise display [ammo remaining for gun] / [total ammo capacity for gun]
        else
        {
            ammoRemainingText.text = weapon.weaponRemainingAmmo.ToString() + " / " + weapon.weaponDetails.weaponAmmoCapacity.ToString();
        }
    }

    /// <summary>
    /// Update ammo clip icons on the UI
    /// </summary>
    /// <param name="weapon"></param>
    private void UpdateAmmoLoadedIcons(Weapon weapon)
    {
        ClearAmmoLoadedIcons();

        // Iterate through remaining ammo in clip
        for (int i = 0; i < weapon.weaponClipRemainingAmmo; i++)
        {
            // Instantiate ammo icon prefab where the parent is the transform of the gameobject that holds all the ammo icons
            GameObject ammoIcon = Instantiate(GameResources.Instance.ammoIconPrefab, ammoHolderTransform);

            // Place ammo icon above previous ammo icon to create a "column" of ammo icons. First ammo icon is at a relative position of 0, 0
            ammoIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, Settings.uiAmmoIconSpacing * i);

            // Add ammo icon to the list of instantiated ammo icons
            ammoIconList.Add(ammoIcon);
        }
    }

    /// <summary>
    /// Destroy ammoIcon references and clear ammoIconList
    /// </summary>
    private void ClearAmmoLoadedIcons()
    {
        // Loop through icon gameobjects and destroy
        foreach (GameObject ammoIcon in ammoIconList)
        {
            Destroy(ammoIcon);
        }

        ammoIconList.Clear();
    }

    /// <summary>
    /// Reload weapon - update the reload bar on the UI
    /// </summary>
    /// <param name="weapon"></param>
    private void UpdateWeaponReloadBar(Weapon weapon)
    {
        if (weapon.weaponDetails.hasInfiniteClipCapacity)
        {
            return;
        }

        StopReloadWeaponCoroutine();
        UpdateReloadText(weapon);

        reloadWeaponCoroutine = StartCoroutine(UpdateWeaponReloadBarRoutine(weapon));
    }

    /// <summary>
    /// Animate reload weapon bar coroutine
    /// </summary>
    /// <param name="currentWeapon"></param>
    /// <returns></returns>
    private IEnumerator UpdateWeaponReloadBarRoutine(Weapon currentWeapon)
    {
        // Set the reload bar to red
        barImage.color = Color.red;

        // Animate the weapon reload bar
        while (currentWeapon.isWeaponReloading)
        {
            // Update reload bar
            float barFill = currentWeapon.weaponReloadTimer / currentWeapon.weaponDetails.weaponReloadTime;

            // Update bar fill
            reloadBar.transform.localScale = new Vector3(barFill, 1f, 1f);

            yield return null;
        }
    }

    /// <summary>
    /// Initialize the weapon reload bar on the UI
    /// </summary>
    private void ResetWeaponReloadBar()
    {
        StopReloadWeaponCoroutine();

        // Set bar image color to green
        barImage.color = Color.green;

        // Set bar scale to 1
        reloadBar.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    /// <summary>
    /// Stop coroutine updating weapon reload progress bar
    /// </summary>
    private void StopReloadWeaponCoroutine()
    {
        // Stop any active weapon reload bar on the UI
        if (reloadWeaponCoroutine != null)
        {
            StopCoroutine(reloadWeaponCoroutine);
        }
    }

    /// <summary>
    /// Update the blinking weapon reload text
    /// </summary>
    /// <param name="weapon"></param>
    private void UpdateReloadText(Weapon weapon)
    {
        // If the weapon has a finite clip capacity and a finite ammo amount but has no ammo left, show OUT OF AMMO text
        if (!weapon.weaponDetails.hasInfiniteClipCapacity && !weapon.weaponDetails.hasInfiniteAmmo && weapon.weaponRemainingAmmo == 0)
        {
            // Set reload bar color to red
            barImage.color = Color.red;

            StopBlinkingOutOfAmmoTextCoroutine();
            StopBlinkingReloadTextCoroutine();

            blinkingOutOfAmmoTextCoroutine = StartCoroutine(StartBlinkingOutOfAmmoTextCoroutine());
        }
        // If the weapon has a finite clip capacity and has no ammo left or is reloading, show blinking RELOAD text
        else if ((!weapon.weaponDetails.hasInfiniteClipCapacity) && (weapon.weaponClipRemainingAmmo <= 0 || weapon.isWeaponReloading))
        {
            // Set reload bar color to red
            barImage.color = Color.red;

            StopBlinkingOutOfAmmoTextCoroutine();
            StopBlinkingReloadTextCoroutine();

            blinkingReloadTextCoroutine = StartCoroutine(StartBlinkingReloadTextCoroutine());
        }
        // If the weapon has an infinite clip capcity or has ammo left and is not reloading, stop blinking RELOAD text
        else
        {
            StopBlinkingOutOfAmmoTextCoroutine();
            StopBlinkingReloadText();
        }
    }

    /// <summary>
    /// Start the coroutine to blink the out of ammo weapon text on and off for 0.3 seconds at a time
    /// </summary>
    /// <returns>Coroutine to blink OUT OF AMMO text</returns>
    private IEnumerator StartBlinkingOutOfAmmoTextCoroutine()
    {
        while (true)
        {
            reloadText.text = "OUT OF AMMO";
            yield return new WaitForSeconds(0.3f);
            reloadText.text = "";
            yield return new WaitForSeconds(0.3f);
        }
    }


    /// <summary>
    /// Stop the blinking out of ammo text (coroutine) and reset the text to empty string
    /// </summary>
    private void StopBlinkingOutOfAmmoText()
    {
        StopBlinkingOutOfAmmoTextCoroutine();

        reloadText.text = "";
    }

    /// <summary>
    /// Stop the blinking out of ammo text coroutine
    /// </summary>
    private void StopBlinkingOutOfAmmoTextCoroutine()
    {
        if (blinkingOutOfAmmoTextCoroutine != null)
        {
            StopCoroutine(blinkingOutOfAmmoTextCoroutine);
        }
    }

    /// <summary>
    /// Start the coroutine to blink the reload weapon text on and off for 0.3 seconds at a time
    /// </summary>
    /// <returns>Coroutine to blink RELOAD text</returns>
    private IEnumerator StartBlinkingReloadTextCoroutine()
    {
        while (true)
        {
            reloadText.text = "RELOAD";
            yield return new WaitForSeconds(0.3f);
            reloadText.text = "";
            yield return new WaitForSeconds(0.3f);
        }
    }

    /// <summary>
    /// Stop the blinking reload text (coroutine) and reset the text to empty string
    /// </summary>
    private void StopBlinkingReloadText()
    {
        StopBlinkingReloadTextCoroutine();

        reloadText.text = "";
    }

    /// <summary>
    /// Stop the blinking reload text coroutine
    /// </summary>
    private void StopBlinkingReloadTextCoroutine()
    {
        if (blinkingReloadTextCoroutine != null)
        {
            StopCoroutine(blinkingReloadTextCoroutine);
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponImage), weaponImage);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoHolderTransform), ammoHolderTransform);
        HelperUtilities.ValidateCheckNullValue(this, nameof(reloadText), reloadText);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoRemainingText), ammoRemainingText);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponNameText), weaponNameText);
        HelperUtilities.ValidateCheckNullValue(this, nameof(reloadBar), reloadBar);
        HelperUtilities.ValidateCheckNullValue(this, nameof(barImage), barImage);
    }
#endif
    #endregion
}
