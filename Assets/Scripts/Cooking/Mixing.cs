using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

using FMODUnity;
using FMOD.Studio;

public class Mixing : MonoBehaviour
{
    [SerializeField] GameObject controlsText;
    [SerializeField] int totalMixes = 7;
    [SerializeField] Vector2 center = new Vector2(0, 0);
    [SerializeField] Slider progressBar;
    [SerializeField] Animator spoonAnim;
    [SerializeField] float spoonAnimSpeed = 2;

    // Used to calculate the radial difference of the mouse each frame
    Vector2 prevPos;

    int mixCount = 0;

    // Tracks the progress of current mix, once this reaches pi, mixCount++ and currentMix = 0
    float currentMix = 0;
    float prevMix = 0;
    const float fullMixValue = Mathf.PI;

    bool canMix = false;
    bool goingClockwise = true;

    bool inPause = false;
    public CookingManager cManager;

    [Header("FMOD SFX")]
    [FMODUnity.EventRef]
    [SerializeField] private string mixingLoopEvent; // example: event:/Sound Effects/Mixing

    private EventInstance mixingInstance;
    private bool mixingSfxStarted = false;

    void Start()
    {
        controlsText.SetActive(true);

        progressBar.minValue = 0;
        progressBar.maxValue = totalMixes * fullMixValue;
        progressBar.value = 0;
        progressBar.gameObject.SetActive(true);

        center = transform.position;

        spoonAnim.speed = 0;
    }

    void Update()
    {
        inPause = cManager.inPause;
        if (inPause)
        {
            PauseMixSfx(true);
            return;
        }
        else
        {
            PauseMixSfx(false);
        }

        // Sets previous position to initially be the mouse position on the first click
        // This is also the moment mixing "starts" from a gameplay perspective
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            prevPos = new Vector2(pos.x - center.x, pos.y - center.y).normalized;
            canMix = true;

            // Start looping SFX when mixing begins (bar begins filling)
            StartMixSfx();
        }

        // Gets the current mouse position and calculates the angle difference between the current and prev
        if (Input.GetMouseButton(0) && canMix)
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Debug.Log(Input.mousePosition);
            Debug.Log(pos);
            CalculateRotation(pos);
            CalculateSpeed();

            // Updates the progress bar
            progressBar.value = mixCount * fullMixValue + Mathf.Clamp(currentMix, 0, fullMixValue);
        }

        // Stops the spoon animation if the player lets go of the button
        // Note: we do NOT stop the looping SFX here, because we want it to keep looping till completion
        if (Input.GetMouseButtonUp(0))
        {
            spoonAnim.speed = 0;
        }

        // Checks if the current mix has done a full rotation, updates mix count and current mix = 0
        if (currentMix >= fullMixValue || currentMix <= -fullMixValue)
        {
            mixCount++;
            currentMix = 0;
            prevMix = 0;
        }

        // If there's been enough mixes, transition to next step
        if (mixCount >= totalMixes)
        {
            StopMixSfx();

            controlsText.SetActive(false);
            progressBar.gameObject.SetActive(false);
            CookingManager.instance.Transition();
            spoonAnim.speed = 0;
            gameObject.SetActive(false);
        }
    }

    void CalculateRotation(Vector2 currentPos)
    {
        float currMinCenterx = currentPos.x - center.x;
        float currMinCentery = currentPos.y - center.y;
        Vector2 currNormalized = new Vector2(currMinCenterx, currMinCentery).normalized;

        float centerCurrentX = 1 - currNormalized.x;
        float centerCurrentY = currNormalized.y;

        float centerPrevX = 1 - prevPos.x;
        float centerPrevY = prevPos.y;

        float currentRadians = Mathf.Atan2(centerCurrentY, centerCurrentX);
        float prevRadians = Mathf.Atan2(centerPrevY, centerPrevX);

        float radians = Mathf.Abs(currentRadians) - Mathf.Abs(prevRadians);

        prevMix = currentMix;
        currentMix += Mathf.Abs(radians);
        prevPos = currNormalized;

        // Checking mixing direction
        CheckMixDirection(prevRadians, currentRadians);
    }

    void CheckMixDirection(float prevRadians, float currentRadians)
    {
        if (prevRadians >= 0 && currentRadians < 0)
        {
            if (prevRadians > 1 && !goingClockwise)
            {
                SwitchAnimDirection();
                goingClockwise = true;
                return;
            }
            else if (goingClockwise)
            {
                SwitchAnimDirection();
                goingClockwise = false;
                return;
            }
        }
        else if (prevRadians <= 0 && currentRadians > 0)
        {
            if (prevRadians < -1 && goingClockwise)
            {
                SwitchAnimDirection();
                goingClockwise = false;
                return;
            }
            else if (!goingClockwise)
            {
                SwitchAnimDirection();
                goingClockwise = true;
                return;
            }
        }
        else
        {
            if (prevRadians < currentRadians && !goingClockwise)
            {
                SwitchAnimDirection();
                goingClockwise = true;
                return;
            }
            else if (prevRadians > currentRadians && goingClockwise)
            {
                SwitchAnimDirection();
                goingClockwise = false;
                return;
            }
        }
    }

    void CalculateSpeed()
    {
        float fractionOfMix = (currentMix - prevMix) / fullMixValue;
        float animSpeed = spoonAnimSpeed * fractionOfMix / Time.deltaTime;
        spoonAnim.speed = animSpeed;
    }

    void SwitchAnimDirection()
    {
        float currAnimTime = spoonAnim.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;

        if (goingClockwise)
        {
            spoonAnim.Play("SpoonAnimation", 0, Mathf.Abs(1 - currAnimTime));
        }
        else
        {
            spoonAnim.Play("ReversedSpoonAnimation", 0, Mathf.Abs(1 - currAnimTime));
        }
    }

    private void StartMixSfx()
    {
        if (mixingSfxStarted) return;
        if (string.IsNullOrEmpty(mixingLoopEvent)) return;

        mixingInstance = RuntimeManager.CreateInstance(mixingLoopEvent);
        mixingInstance.start();
        mixingSfxStarted = true;
    }

    private void StopMixSfx()
    {
        if (!mixingSfxStarted) return;

        if (mixingInstance.isValid())
        {
            mixingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            mixingInstance.release();
        }

        mixingSfxStarted = false;
    }

    private void PauseMixSfx(bool pause)
    {
        if (!mixingSfxStarted) return;
        if (!mixingInstance.isValid()) return;

        mixingInstance.setPaused(pause);
    }

    private void OnDisable()
    {
        StopMixSfx();
    }
}