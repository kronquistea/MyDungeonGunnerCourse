using UnityEngine;
using UnityEngine.Rendering.Universal; // 2D lights should no longer be experimental
using UnityEngine.Experimental.Rendering.Universal;

[DisallowMultipleComponent]

public class LightFlicker : MonoBehaviour
{
    private Light2D light2D;
    [SerializeField] private float lightIntensityMin;
    [SerializeField] private float lightIntensityMax;
    [SerializeField] private float lightFlickerTimeMin;
    [SerializeField] private float lightFlickerTimeMax;
    private float lightFlickerTime;

    private void Awake()
    {
        // Load components
        light2D = GetComponentInChildren<Light2D>();
    }

    private void Start()
    {
        lightFlickerTime = Random.Range(lightFlickerTimeMin, lightFlickerTimeMax);
    }

    private void Update()
    {
        // Check if a light2D object exists on the gameobject
        if (light2D == null)
        {
            return;
        }

        // Update remaining flicker duration
        lightFlickerTime -= Time.deltaTime;

        // Check if the current duration for flickering is done
        if (lightFlickerTime < 0f)
        {
            // Set new flicker time
            lightFlickerTime = Random.Range(lightFlickerTimeMin, lightFlickerTimeMax);

            // Randomize the intensity of the light for the new flicker duration
            RandomizeLightIntensity();
        }
    }

    /// <summary>
    /// Get a random intensity for the light2D object
    /// </summary>
    private void RandomizeLightIntensity()
    {
        light2D.intensity = Random.Range(lightIntensityMin, lightIntensityMax);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(lightIntensityMin), lightIntensityMin, nameof(lightIntensityMax), lightIntensityMax, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(lightFlickerTimeMin), lightFlickerTimeMin, nameof(lightFlickerTimeMax), lightFlickerTimeMax, false);
    }
#endif
    #endregion

}
