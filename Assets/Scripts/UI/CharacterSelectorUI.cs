using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]

public class CharacterSelectorUI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate this with the child CharacterSelector gameobject")]
    #endregion
    [SerializeField] private Transform characterSelector;

    #region Tooltip
    [Tooltip("Populate with the TMP component on the PlayerNameInput gameobject")]
    #endregion
    [SerializeField] private TMP_InputField playerNameInput;

    private List<PlayerDetailsSO> playerDetailsList;
    private GameObject playerSelectionPrefab;
    private CurrentPlayerSO currentPlayer;
    private List<GameObject> playerCharacterGameObjectList = new List<GameObject>();
    private Coroutine coroutine;
    private int selectedPlayerIndex = 0;
    private float offset = 4f;

    private void Awake()
    {
        // Load resources
        playerSelectionPrefab = GameResources.Instance.playerSelectionPrefab;
        playerDetailsList = GameResources.Instance.playerDetailsList;
        currentPlayer = GameResources.Instance.currentPlayer;
    }

    private void Start()
    {
        // Instantiate player characters in player details list
        for (int i = 0; i < playerDetailsList.Count; i++)
        {
            // Instantiate a new player selection game object
            GameObject playerSelectionObject = Instantiate(playerSelectionPrefab, characterSelector);
            
            // Add the player selection game object to the list
            playerCharacterGameObjectList.Add(playerSelectionObject);

            // Set player selection game object local position (position within the characterSelector gameobject)
            playerSelectionObject.transform.localPosition = new Vector3(i * offset, 0f, 0f);

            // Populate the player details for the current character
            PopulatePlayerDetails(playerSelectionObject.GetComponent<PlayerSelectionUI>(), playerDetailsList[i]);
        }

        // Set the text for the player to the specified player name
        playerNameInput.text = currentPlayer.playerName;

        // Initialize the current player (starting as the General)
        currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];
    }

    /// <summary>
    /// Populate player character details for display (sprites and animator)
    /// </summary>
    /// <param name="playerSelection"></param>
    /// <param name="playerDetails"></param>
    private void PopulatePlayerDetails(PlayerSelectionUI playerSelection, PlayerDetailsSO playerDetails)
    {
        playerSelection.playerHandSpriteRenderer.sprite = playerDetails.playerHandSprite;
        playerSelection.playerHandNoWeaponSpriteRenderer.sprite = playerDetails.playerHandSprite;
        playerSelection.playerWeaponSpriteRenderer.sprite = playerDetails.startingWeapon.weaponSprite;
        playerSelection.animator.runtimeAnimatorController = playerDetails.runtimeAnimatorController;
    }

    /// <summary>
    /// Select the next character - linked to by the OnClick event in the next character button
    /// </summary>
    public void NextCharacter()
    {
        // Check if the selected player is the last in the list
        if (selectedPlayerIndex >= playerDetailsList.Count - 1)
        {
            // Do nothing
            return;
        }

        // Increment to reference next character
        selectedPlayerIndex++;

        // Set the current player details to be the next character in the list
        currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];

        // Visually move the characters to the side so the user can see which character is currently selected
        MoveToSelectedCharacter(selectedPlayerIndex);
    }

    public void PreviousCharacter()
    {
        // Check if the player is on the first character in the list
        if (selectedPlayerIndex == 0)
        {
            // Do nothing
            return;
        }

        // Decrement to reference previous character
        selectedPlayerIndex--;

        // Set the current player details to be the previous character in the list
        currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];

        // Visually move the characters to the side so the user can see which character is currently selected
        MoveToSelectedCharacter(selectedPlayerIndex);
    }

    /// <summary>
    /// Update the UI so the next character is shown on the screen to the user
    /// </summary>
    /// <param name="selectedPlayerIndex"></param>
    private void MoveToSelectedCharacter(int selectedPlayerIndex)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        coroutine = StartCoroutine(MoveToSelectedCharacterRoutine(selectedPlayerIndex));
    }

    /// <summary>
    /// Coroutine to visually move the characters when the next/previous character buttons are pressed
    /// </summary>
    /// <param name="index"></param>
    /// <returns>Coroutine</returns>
    private IEnumerator MoveToSelectedCharacterRoutine(int index)
    {
        // Current x position based on local position of character selector
        float currentLocalXPosition = characterSelector.localPosition.x;

        // Target x position based on the character that is being moved to and the offset (we are basically moving the anchor point left or right)
        float targetLocalXPosition = index * offset * characterSelector.localScale.x * -1f;
    
        // Move the entire character selector position to the left or right (next/previous character) until the next character is being shown
        while (Mathf.Abs(currentLocalXPosition - targetLocalXPosition) > 0.01f)
        {
            // Calculate the new local position as the lerp between the current location to the next over
            // (this should mean that as the character move "farther", they move slower towards their final position)
            currentLocalXPosition = Mathf.Lerp(currentLocalXPosition, targetLocalXPosition, Time.deltaTime * 10f);

            // Set the position of the character selector based on the newly lerp position
            characterSelector.localPosition = new Vector3(currentLocalXPosition, characterSelector.localPosition.y, 0f);
            yield return null;
        }

        // Finish off the character selector position (because the while loop will not enable it to actually get to the target x position)
        characterSelector.localPosition = new Vector3(targetLocalXPosition, characterSelector.localPosition.y, 0f);
    }

    /// <summary>
    /// Update player name - linked to through input text field in UI
    /// </summary>
    public void UpdatePlayerName()
    {
        playerNameInput.text = playerNameInput.text.ToUpper();

        // Set player name to player name input capitalized
        currentPlayer.playerName = playerNameInput.text;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(characterSelector), characterSelector);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerNameInput), playerNameInput);
    }
#endif
    #endregion
}
