using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SliderController:
/// - Fire slider value comes from BellowsController.Value01 (0..1).
/// - Fire fill color:
///     * Red by default
///     * Green ONLY when fire is within [greenMin, greenMax] (inclusive)
///     * Red again when below greenMin or above greenMax (and switches back dynamically)
/// - Progress slider advances ONLY while fire is within the green window.
///   Outside the window, progress does not change (no fill, no decay).
/// </summary>
public class SliderController : MonoBehaviour
{
    [Header("References")]

    [Tooltip("BellowsController that drives the fire power value (0..1).")]
    [SerializeField] private BellowsController fireBellows;
    
    [SerializeField] private Slider progressSlider;

    [Header("Green Window (inclusive)")]
    [Range(0f, 1f)]
    [SerializeField] private float greenMin = 0.50f;

    [Range(0f, 1f)]
    [SerializeField] private float greenMax = 0.75f;

    [Header("Colors")]
    [SerializeField] private Color redColor = Color.red;
    [SerializeField] private Color greenColor = Color.green;

    [Header("Progress Fill")]
    [SerializeField] private float progressPerSecond = 0.12f;

    [SerializeField] private bool clampProgressAtFull = true;

    public bool InGreenWindow { get; private set; }

    public float Fire01 => fireBellows != null ? Mathf.Clamp01(fireBellows.Value01) : 0f;

    [SerializeField] private Slider fireSlider;
    private Image fireFillImage;

    private void Awake()
    {
        if (fireSlider != null && fireSlider.fillRect != null)
            fireFillImage = fireSlider.fillRect.GetComponent<Image>();
    }

    private void Update()
    {
        float fire01 = Fire01;

        UpdateFireWindowAndColor(fire01);

        // Progress advances only while in the green window.
        if (InGreenWindow && progressSlider)
        {
            float next = progressSlider.value + progressPerSecond * Time.deltaTime;
            if (clampProgressAtFull) next = Mathf.Clamp01(next);
            progressSlider.value = next;
        }
        // Outside green window: do nothing (progress stays exactly as-is).
    }

    public void ResetProgress()
    {
        if (progressSlider != null) progressSlider.value = 0f;
    }

    private void UpdateFireWindowAndColor(float fire01)
    {
        InGreenWindow = fire01 >= greenMin && fire01 <= greenMax;

        if (fireFillImage)
        {
            fireFillImage.color = InGreenWindow ? greenColor : redColor;
        }
    }
}
