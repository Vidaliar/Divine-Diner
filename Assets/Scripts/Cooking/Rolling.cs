using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

using FMODUnity;
using FMOD.Studio;

public class Rolling : MonoBehaviour
{
    [SerializeField] Transform rollingPin;
    [SerializeField] GameObject dough;
    [SerializeField] int totalRolls = 3;
    [SerializeField] float rollSpeed = 3f;

    //The dough might already be scaled, so the end scale will be og scale + size diff
    [SerializeField] float xSizeDiff = 5f;
    [SerializeField] float ySizeDiff = 2f;

    int rolls = 0;

    //The values to add to the scale for each roll
    float xSizeFrac;
    float ySizeFrac;

    bool canRoll = false;
    bool nextIsRight = true;

    bool inPause = false;
    public CookingManager cManager;

    [Header("FMOD SFX")]
    [FMODUnity.EventRef]
    [SerializeField] private string rollingLoopEvent; // example: event:/Sound Effects/Rolling Dough (New)

    private EventInstance rollingInstance;
    private bool rollingSfxStarted = false;

    void Start()
    {
        xSizeFrac = xSizeDiff / totalRolls;
        ySizeFrac = ySizeDiff / totalRolls;
    }

    void Update()
    {
        inPause = cManager.inPause;

        if (inPause)
        {
            PauseRollingSfx(true);
            return; // Makes sure game isn't paused before anything happens
        }
        else
        {
            PauseRollingSfx(false);
        }

        //Dough collider bounds
        float doughMinBound = dough.GetComponent<Collider2D>().bounds.min.x;
        float doughMaxBound = dough.GetComponent<Collider2D>().bounds.max.x;

        //Allows the player to start rolling after the any previous prep
        if (Input.GetMouseButtonDown(0))
        {
            canRoll = true;
            StartRollingSfx();
        }

        // Stop loop as soon as player lets go
        if (Input.GetMouseButtonUp(0))
        {
            StopRollingSfx();
        }

        if (Input.GetMouseButton(0) && canRoll)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //Normalized vector to move the pin
            Vector2 direction = new Vector2(mousePos.x - rollingPin.position.x, mousePos.y - rollingPin.position.y).normalized;

            //The x value to set the pin x value as
            float newX = rollingPin.position.x + (direction.x * rollSpeed * Time.deltaTime);
            float clampX = Mathf.Clamp(newX, doughMinBound, doughMaxBound);

            rollingPin.position = new Vector2(clampX, rollingPin.position.y);

            //Checks if pin hit the correct side, and if so, increments rolls and expand dough
            if ((clampX >= doughMaxBound && nextIsRight) || (clampX <= doughMinBound && !nextIsRight))
            {
                nextIsRight = !nextIsRight;
                rolls++;
                dough.transform.localScale = new Vector2(
                    dough.transform.localScale.x + xSizeFrac,
                    dough.transform.localScale.y + ySizeFrac
                );
            }
        }

        //Transition to next step if player has rolled enough
        if (rolls >= totalRolls)
        {
            StopRollingSfx();
            CookingManager.instance.Transition();
            this.gameObject.SetActive(false);
        }
    }

    private void StartRollingSfx()
    {
        if (rollingSfxStarted) return;
        if (string.IsNullOrEmpty(rollingLoopEvent)) return;

        rollingInstance = RuntimeManager.CreateInstance(rollingLoopEvent);
        rollingInstance.start();
        rollingSfxStarted = true;
    }

    private void StopRollingSfx()
    {
        if (!rollingSfxStarted) return;

        if (rollingInstance.isValid())
        {
            rollingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            rollingInstance.release();
        }

        rollingSfxStarted = false;
    }

    private void PauseRollingSfx(bool pause)
    {
        if (!rollingSfxStarted) return;
        if (!rollingInstance.isValid()) return;

        rollingInstance.setPaused(pause);
    }

    private void OnDisable()
    {
        StopRollingSfx();
    }
}