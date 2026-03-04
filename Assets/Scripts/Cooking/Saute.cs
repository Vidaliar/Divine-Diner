using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

using FMODUnity;
using FMOD.Studio;

public class Saute : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] Transform pan;
    [SerializeField] Slider progressBar;
    [SerializeField] GameObject instructions;
    [SerializeField] GameObject instuctArrows;

    [Header("Numerical values")]
    [SerializeField] int maxMoves = 16;
    [SerializeField] float maxTime = 4f;
    [SerializeField] int numSections = 4;
    // [SerializeField] float sauteSpeed;

    int moves = 0;  // back-and-forth moves in the current section
    int totalMoves = 0;
    int moveSection;

    float minX;
    float maxX;

    float distance = 2f;
    float currDistance = 0;

    float timer = 0f;
    float totalTime = 0f;
    float timeSection;

    Vector2 prevPosition;

    bool canSaute = false;

    bool inPause = false;
    public CookingManager cManager;

    [Header("FMOD SFX")]
    [FMODUnity.EventRef]
    [SerializeField] private string sauteLoopEvent; // assign in Inspector

    private EventInstance sauteInstance;
    private bool sauteSfxStarted = false;

    void Start()
    {
        maxX = pan.position.x + 1f;
        minX = maxX - 2f;

        timeSection = maxTime / numSections;
        moveSection = maxMoves / numSections;

        progressBar.minValue = 0;
        progressBar.maxValue = maxTime;
        progressBar.value = 0;
        progressBar.gameObject.SetActive(true);

        prevPosition = pan.position;
    }

    void Update()
    {
        inPause = cManager.inPause;
        if (inPause)
        {
            PauseSauteSfx(true);
            return;
        }
        else
        {
            PauseSauteSfx(false);
        }

        // Allows the player to start sauteing after any previous prep
        if (Input.GetMouseButtonDown(0))
        {
            canSaute = true;
        }

        // Active saute window: only when the section timer is done
        if (Input.GetMouseButton(0) && canSaute && timer >= timeSection)
        {
            StartSauteSfx(); // start loop when saute action begins

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            float clampX = Mathf.Clamp(mousePos.x, minX, maxX);
            pan.position = new Vector2(clampX, pan.position.y);

            currDistance += Mathf.Abs(prevPosition.x - pan.position.x);
            prevPosition = pan.position;

            if (currDistance >= distance)
            {
                moves++;
                currDistance = 0;
            }
        }

        // Show instructions only during active window
        if (timer >= timeSection)
        {
            instructions.SetActive(true);
            instuctArrows.SetActive(true);
        }
        else
        {
            instructions.SetActive(false);
            instuctArrows.SetActive(false);
        }

        // Timer fills progress bar during the "wait" part of each section
        if (timer < timeSection)
        {
            timer += Time.deltaTime;
            totalTime += Time.deltaTime;
            progressBar.value = totalTime;
        }

        // End of a section (your original logic)
        if (moves == numSections)
        {
            totalMoves += moves;
            timer = 0;
            moves = 0;
        }

        // Done: transition to next step
        if (totalMoves >= maxMoves)
        {
            StopSauteSfx();

            CookingManager.instance.Transition();
            progressBar.gameObject.SetActive(false);
            instructions.SetActive(false);
            instuctArrows.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    private void StartSauteSfx()
    {
        if (sauteSfxStarted) return;
        if (string.IsNullOrEmpty(sauteLoopEvent)) return;

        sauteInstance = RuntimeManager.CreateInstance(sauteLoopEvent);
        sauteInstance.start();
        sauteSfxStarted = true;
    }

    private void StopSauteSfx()
    {
        if (!sauteSfxStarted) return;

        if (sauteInstance.isValid())
        {
            sauteInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            sauteInstance.release();
        }

        sauteSfxStarted = false;
    }

    private void PauseSauteSfx(bool pause)
    {
        if (!sauteSfxStarted) return;
        if (!sauteInstance.isValid()) return;

        sauteInstance.setPaused(pause);
    }

    private void OnDisable()
    {
        StopSauteSfx();
    }
}