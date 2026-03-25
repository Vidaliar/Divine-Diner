using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SliderController:
/// - Fills a time slider (0..1) over a fixed duration.
/// - While time slider is NOT full:
///     * Evaluates the current wind value (0..1) into 4 zones:
///         High: [0.85, 1.00] -> Orange (fill), score += highRate * dt
///         Mid : [0.70, 0.85) -> Yellow (fill), score += midRate  * dt
///         Low : [0.50, 0.70) -> Green  (fill), score += lowRate  * dt
///         None: [0.00, 0.50) -> noneColor (fill), score += 0
/// - Once time slider reaches full (1.0), score growth stops immediately.
/// - Optional: Creates/updates segmented background zones behind the wind slider.
/// 
/// Requires:
/// - BellowsController reference to read Value01.
/// - Wind slider Fill Image (windFillImage) to change fill color.
/// - Time slider (timeSlider) to fill over time.
/// - (Optional) 4 background Images under a shared RectTransform (windZoneRoot).
/// </summary>
public class SliderController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BellowsController bellows;
    [SerializeField] private Slider timeSlider;

    [Tooltip("Image component on the WIND slider's Fill object (WindSlider/Fill Area/Fill).")]
    [SerializeField] private Image windFillImage;

    [Header("Round Timer")]
    [Tooltip("Seconds for the time slider to fill from 0 to 1.")]
    [SerializeField] private float roundDurationSeconds = 10f;

    [Header("Zone Thresholds (0..1)")]
    [SerializeField] private float lowThreshold = 0.50f;   // >= lowThreshold is Low
    [SerializeField] private float midThreshold = 0.70f;   // >= midThreshold is Mid
    [SerializeField] private float highThreshold = 0.85f;  // >= highThreshold is High

    [Header("Wind Fill Colors")]
    [SerializeField] private Color highFillColor = new Color(1f, 0.55f, 0f, 1f); // orange
    [SerializeField] private Color midFillColor = Color.yellow;
    [SerializeField] private Color lowFillColor = Color.green;
    [SerializeField] private Color noneFillColor = Color.gray;

    [Header("Score Rates (points per second)")]
    [SerializeField] private float lowRate = 1f;
    [SerializeField] private float midRate = 2f;
    [SerializeField] private float highRate = 3f;

    [Header("Optional: Wind Slider Segmented Background")]
    [Tooltip("Enable to layout and color segmented background zones behind the wind slider.")]
    [SerializeField] private bool enableWindZoneBackground = true;

    [Tooltip("A RectTransform that stretches over the full bar area (behind wind fill).")]
    [SerializeField] private RectTransform windZoneRoot;

    [Tooltip("Background Image for None zone: [0, lowThreshold).")]
    [SerializeField] private Image noneZoneBg;

    [Tooltip("Background Image for Low zone: [lowThreshold, midThreshold).")]
    [SerializeField] private Image lowZoneBg;

    [Tooltip("Background Image for Mid zone: [midThreshold, highThreshold).")]
    [SerializeField] private Image midZoneBg;

    [Tooltip("Background Image for High zone: [highThreshold, 1].")]
    [SerializeField] private Image highZoneBg;

    [Tooltip("Background colors (often use lower alpha than fill).")]
    [SerializeField] private Color noneBgColor = new Color(0.3f, 0.3f, 0.3f, 0.35f);
    [SerializeField] private Color lowBgColor = new Color(0f, 1f, 0f, 0.25f);
    [SerializeField] private Color midBgColor = new Color(1f, 1f, 0f, 0.25f);
    [SerializeField] private Color highBgColor = new Color(1f, 0.55f, 0f, 0.25f);

    // Runtime
    private float time01;
    private float score;
    private bool finished;

    /// <summary>Hidden score accumulated in this round.</summary>
    public float Score => score;

    /// <summary>Timer progress in 0..1.</summary>
    public float Time01 => time01;

    /// <summary>True when the time slider is full (round ended).</summary>
    public bool Finished => finished;

    private enum Zone { None, Low, Mid, High }

    private void Awake()
    {
        InitTimeSlider();
        if (enableWindZoneBackground) LayoutAndColorZoneBackground();
        ApplyWindFillColor(GetWindValueSafe());
    }

    private void Update()
    {
        if (finished)
        {
            //Slider is a timer, not a progress bar
            //This works for having attributes, but without attributes we need a progress bar instead
            //Progress bar only increments while the wind slider is in the 1 'good' section
            //Once the progress bar is full/finished transition to next minigame
            Debug.Log("Call transition here");
        } 

        float dt = Time.deltaTime;

        if (roundDurationSeconds <= 0f)
        {
            // Degenerate case: immediately finish.
            time01 = 1f;
            if (timeSlider != null) timeSlider.value = 1f;
            finished = true;
            return;
        }

        // Advance timer with clamping so scoring stops exactly at full.
        float delta01 = dt / roundDurationSeconds;
        float remain01 = 1f - time01;
        float applied01 = Mathf.Min(delta01, remain01);

        time01 += applied01;
        if (timeSlider != null) timeSlider.value = time01;

        // Effective dt that happened before reaching full.
        float effectiveDt = applied01 * roundDurationSeconds;

        // While not full, update wind fill color and score growth.
        float wind = GetWindValueSafe();
        Zone zone = EvaluateZone(wind);

        ApplyWindFillColorByZone(zone);
        score += GetRateByZone(zone) * effectiveDt;

        if (time01 >= 0.999999f)
        {
            time01 = 1f;
            if (timeSlider != null) timeSlider.value = 1f;
            finished = true;
        }
    }

    public void ResetRound()
    {
        time01 = 0f;
        score = 0f;
        finished = false;

        if (timeSlider != null) timeSlider.value = 0f;
        ApplyWindFillColor(GetWindValueSafe());
    }

    private void InitTimeSlider()
    {
        if (timeSlider == null) return;
        timeSlider.minValue = 0f;
        timeSlider.maxValue = 1f;
        timeSlider.value = 0f;
        timeSlider.interactable = false;
        timeSlider.gameObject.SetActive(true);  //From Makena - I assume the slider is off since some minigames (currently) don't use it
    }

    private float GetWindValueSafe()
    {
        return bellows != null ? Mathf.Clamp01(bellows.Value01) : 0f;
    }

    private Zone EvaluateZone(float wind01)
    {
        if (wind01 >= highThreshold) return Zone.High;
        if (wind01 >= midThreshold) return Zone.Mid;
        if (wind01 >= lowThreshold) return Zone.Low;
        return Zone.None;
    }

    private float GetRateByZone(Zone zone)
    {
        switch (zone)
        {
            case Zone.High: return highRate;
            case Zone.Mid: return midRate;
            case Zone.Low: return lowRate;
            default: return 0f;
        }
    }

    private void ApplyWindFillColor(float wind01)
    {
        ApplyWindFillColorByZone(EvaluateZone(wind01));
    }

    private void ApplyWindFillColorByZone(Zone zone)
    {
        if (windFillImage == null) return;

        switch (zone)
        {
            case Zone.High: windFillImage.color = highFillColor; break;
            case Zone.Mid: windFillImage.color = midFillColor; break;
            case Zone.Low: windFillImage.color = lowFillColor; break;
            default: windFillImage.color = noneFillColor; break;
        }
    }

    /// <summary>
    /// Layouts 4 background Images vertically (bottom->top) using thresholds,
    /// and assigns background colors.
    /// windZoneRoot and all 4 Images must be provided.
    /// </summary>
    public void LayoutAndColorZoneBackground()
    {
        if (!enableWindZoneBackground) return;
        if (windZoneRoot == null) return;
        if (noneZoneBg == null || lowZoneBg == null || midZoneBg == null || highZoneBg == null) return;

        // Ensure thresholds are sane.
        float t0 = Mathf.Clamp01(lowThreshold);
        float t1 = Mathf.Clamp01(midThreshold);
        float t2 = Mathf.Clamp01(highThreshold);

        // Enforce ordering to avoid inverted zones.
        if (t1 < t0) t1 = t0;
        if (t2 < t1) t2 = t1;

        // Layout: None [0, t0), Low [t0, t1), Mid [t1, t2), High [t2, 1]
        SetVerticalZoneRect(noneZoneBg.rectTransform, 0f, t0);
        SetVerticalZoneRect(lowZoneBg.rectTransform, t0, t1);
        SetVerticalZoneRect(midZoneBg.rectTransform, t1, t2);
        SetVerticalZoneRect(highZoneBg.rectTransform, t2, 1f);

        // Apply colors (usually use lower alpha so fill is still readable).
        noneZoneBg.color = noneBgColor;
        lowZoneBg.color = lowBgColor;
        midZoneBg.color = midBgColor;
        highZoneBg.color = highBgColor;
    }

    private void SetVerticalZoneRect(RectTransform rt, float yMin01, float yMax01)
    {
        if (rt == null) return;

        rt.anchorMin = new Vector2(0f, yMin01);
        rt.anchorMax = new Vector2(1f, yMax01);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }
}
