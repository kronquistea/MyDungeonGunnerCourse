using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(MaterializeEffect))]

public class ChestItem : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private TextMeshPro textTMP;
    private MaterializeEffect materializeEffect;
    [HideInInspector] public bool isItemMaterialized = false;

    private void Awake()
    {
        // Load components
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        textTMP = GetComponentInChildren<TextMeshPro>();
        materializeEffect = GetComponent<MaterializeEffect>();
    }

    /// <summary>
    /// Intialize the chest item
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="text"></param>
    /// <param name="spawnPosition"></param>
    /// <param name="materializeColor"></param>
    public void Initialize(Sprite sprite, string text, Vector3 spawnPosition, Color materializeColor)
    {
        spriteRenderer.sprite = sprite;
        transform.position = spawnPosition;

        StartCoroutine(MaterializeItem(materializeColor, text));
    }

    /// <summary>
    /// Coroutine to material the chest item
    /// </summary>
    /// <param name="materializeColor"></param>
    /// <param name="text"></param>
    /// <returns>Coroutine</returns>
    private IEnumerator MaterializeItem(Color materializeColor, string text)
    {
        // Must pass in an array of sprite renderers, however we only have one, so create a sprite renderer array with the one sprite renderer
        SpriteRenderer[] spriteRendererArray = new SpriteRenderer[] { spriteRenderer };

        // Materialize the actual sprite renderer (health icon, bullet icon, or weapon sprites)
        yield return StartCoroutine(materializeEffect.MaterializeRoutine(GameResources.Instance.materializeShader, materializeColor, 1f, spriteRendererArray, GameResources.Instance.litMaterial));

        isItemMaterialized = true;

        textTMP.text = text;
    }
}
