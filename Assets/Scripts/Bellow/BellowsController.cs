using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BellowsController controls a wind gauge (0..1) driven by mouse input.
/// 
/// Behavior:
/// - Valid input starts only when Left Mouse Button is pressed on a specific Collider2D (validClickCollider).
/// - On press: instantly add a fixed step (clickStep).
/// - While holding: continuously add (holdRisePerSecond * deltaTime) until the TOTAL gain of this hold
///   reaches maxGainPerHold (this total includes clickStep).
/// - After reaching the per-hold cap: any additional holding time is treated as "no interaction",
///   meaning the gauge will start falling even if the button is still held.
/// - When not interacting: the gauge falls toward 0 at fallPerSecond.
/// 
/// Unity version: 2022.3 (legacy Input via Input.GetMouseButton*).
/// </summary>
public class BellowsController : MonoBehaviour
{
    [Header("UI Binding")]
    [SerializeField] private Slider windSlider;         // Expected range: 0~1
    [SerializeField] private bool updateSlider = true;

    [Header("Gauge Value")]
    [Range(0f, 1f)]
    [SerializeField] private float value01 = 0f;

    [Tooltip("How fast the gauge falls per second when not interacting (in 0..1 space).")]
    [SerializeField] private float fallPerSecond = 0.18f;

    [Header("Click + Hold Behavior")]
    [Tooltip("Instant gain applied on mouse press (valid click only).")]
    [SerializeField] private float clickStep = 0.06f;

    [Tooltip("Continuous gain per second while holding (until per-hold cap is reached).")]
    [SerializeField] private float holdRisePerSecond = 0.25f;

    [Tooltip("Maximum total gain allowed per single hold (includes clickStep + hold gains).")]
    [SerializeField] private float maxGainPerHold = 0.22f;

    [Header("Valid Click Area")]
    [Tooltip("Only clicks that start on this Collider2D are considered valid.")]
    [SerializeField] private Collider2D validClickCollider;

    [Tooltip("Camera used to convert screen mouse position to world position. Defaults to Camera.main.")]
    [SerializeField] private Camera worldCamera;

    // Runtime state
    private bool holding;
    private bool holdCapped;
    private float holdGain;
    public float Value01 => value01;
    public bool IsHolding => holding;
    public bool HoldReachedCap => holdCapped;

    private void Awake()
    {
        if (worldCamera == null) worldCamera = Camera.main;
        if (windSlider != null) windSlider.gameObject.SetActive(true);
        ApplyUI();
    }

    private void Update()
    {
        HandleInput();
        HandleFall();
        ApplyUI();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseOnValidCollider())
            {
                holding = true;
                holdCapped = false;
                holdGain = 0f;

                // Instant step on press.
                AddThisHold(clickStep);
            }
        }

        // Continuous gain while holding and not capped.
        if (holding && Input.GetMouseButton(0))
        {
            if (!holdCapped)
            {
                float add = holdRisePerSecond * Time.deltaTime;
                AddThisHold(add);
            }
        }

        // End hold on release.
        if (holding && Input.GetMouseButtonUp(0))
        {
            holding = false;
            holdCapped = false;
            holdGain = 0f;
        }
    }

    private void AddThisHold(float amount)
    {
        if (amount <= 0f || holdCapped) return;

        float remain = Mathf.Max(0f, maxGainPerHold - holdGain);
        float actual = Mathf.Min(amount, remain);

        if (actual > 0f)
        {
            value01 = Mathf.Clamp01(value01 + actual);
            holdGain += actual;
        }

        if (holdGain >= maxGainPerHold - 0.0001f)
        {
            holdCapped = true;
        }
    }

    private void HandleFall()
    {
        bool interacting = holding && Input.GetMouseButton(0) && !holdCapped;

        if (!interacting)
        {
            value01 = Mathf.MoveTowards(value01, 0f, fallPerSecond * Time.deltaTime);
        }
    }

    private bool IsMouseOnValidCollider()
    {
        if (validClickCollider == null) return false;
        if (worldCamera == null) return false;

        Vector3 world = worldCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 point = new Vector2(world.x, world.y);

        return validClickCollider.OverlapPoint(point);
    }

    private void ApplyUI()
    {
        if (!updateSlider || windSlider == null) return;
        windSlider.value = value01;
    }

    public void Transition()
    {
        windSlider.gameObject.SetActive(false);
        CookingManager.instance.Transition();
        gameObject.SetActive(false);
    }
}
