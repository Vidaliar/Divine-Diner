using UnityEngine;
using UnityEngine.UI;

public class AffectionSliderGroup : MonoBehaviour
{
    public enum CharacterType
    {
        Zeus,
        Hermes,
        Hephaestus
    }

    [Header("Target")]
    [SerializeField] private StateProvider stateProvider;
    [SerializeField] private CharacterType characterType;

    [Header("Sliders")]
    [SerializeField] private Slider lowSlider;
    [SerializeField] private Slider mediumSlider;
    [SerializeField] private Slider highSlider;

    [Header("Refresh")]
    [SerializeField] private bool refreshOnEnable = true;
    [SerializeField] private bool refreshContinuously = false;

    private void Awake()
    {
        if (stateProvider == null)
        {
            stateProvider = FindObjectOfType<StateProvider>();
        }

        SetupSlider(lowSlider);
        SetupSlider(mediumSlider);
        SetupSlider(highSlider);
    }

    private void OnEnable()
    {
        if (refreshOnEnable)
        {
            Refresh();
        }
    }

    private void Update()
    {
        if (refreshContinuously)
        {
            Refresh();
        }
    }

    [ContextMenu("Refresh")]
    public void Refresh()
    {
        if (stateProvider == null)
        {
            stateProvider = FindObjectOfType<StateProvider>();
            if (stateProvider == null) return;
        }

        int currentValue = GetCurrentFavor();
        ApplyToSliders(currentValue);
    }

    private int GetCurrentFavor()
    {
        switch (characterType)
        {
            case CharacterType.Zeus:
                return stateProvider.zeus;

            case CharacterType.Hermes:
                return stateProvider.hermes;

            case CharacterType.Hephaestus:
                return stateProvider.hephaestus;

            default:
                return 0;
        }
    }

    private void ApplyToSliders(int currentValue)
    {
        int lowMin, lowMax;
        int mediumMin, mediumMax;
        int highMin, highMax;

        GetRanges(out lowMin, out lowMax, out mediumMin, out mediumMax, out highMin, out highMax);

        currentValue = Mathf.Clamp(currentValue, lowMin, highMax);

        SetSliderValue(lowSlider, CalculateSegmentFill(currentValue, lowMin, lowMax));
        SetSliderValue(mediumSlider, CalculateSegmentFill(currentValue, mediumMin, mediumMax));
        SetSliderValue(highSlider, CalculateSegmentFill(currentValue, highMin, highMax));
    }

    private void GetRanges(
        out int lowMin, out int lowMax,
        out int mediumMin, out int mediumMax,
        out int highMin, out int highMax)
    {
        switch (characterType)
        {
            case CharacterType.Zeus:
                lowMin = 0;   lowMax = 18;
                mediumMin = 19; mediumMax = 27;
                highMin = 28; highMax = 61;
                break;

            case CharacterType.Hermes:
                lowMin = 0;   lowMax = 14;
                mediumMin = 15; mediumMax = 28;
                highMin = 29; highMax = 44;
                break;

            case CharacterType.Hephaestus:
                lowMin = 0;   lowMax = 18;
                mediumMin = 19; mediumMax = 28;
                highMin = 29; highMax = 39;
                break;

            default:
                lowMin = 0;   lowMax = 0;
                mediumMin = 0; mediumMax = 0;
                highMin = 0; highMax = 0;
                break;
        }
    }

    private float CalculateSegmentFill(int currentValue, int segmentMin, int segmentMax)
    {
        if (currentValue < segmentMin)
            return 0f;

        if (currentValue > segmentMax)
            return 1f;

        if (segmentMax <= segmentMin)
            return 1f;

        return Mathf.InverseLerp(segmentMin, segmentMax, currentValue);
    }

    private void SetupSlider(Slider slider)
    {
        if (slider == null) return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.interactable = false;
    }

    private void SetSliderValue(Slider slider, float value)
    {
        if (slider == null) return;
        slider.value = Mathf.Clamp01(value);
    }
    
}