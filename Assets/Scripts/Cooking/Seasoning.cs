using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;


public class Seasoning : MonoBehaviour
{
    [SerializeField] GameObject controlsText;
    [SerializeField] Transform shaker;
    [SerializeField] ParticleSystem particleSystem;    //Currently used to show the seasoning from the shaker
    [SerializeField] float movementOffset = 1f;     //How much the shaker should move back and forth
    [SerializeField] int totalShakes = 10;

    [Header("FMOD SFX")]
    [FMODUnity.EventRef]
    [SerializeField] private string seasoningLoopEvent;

    private EventInstance seasoningInstance;
    private bool seasoningStarted;

    Vector2 startPos;
    
    int shakeCount = 0;
    bool nextKeyIsD;    //Keeps track if the next key to press is D, if false it's A

    bool inPause = false;
    public CookingManager cManager;
    void Start()
    {
        controlsText.SetActive(true);
        startPos = shaker.position;
    }

    void Update()
    {
        inPause = cManager.inPause;
        if (inPause) {
            if (seasoningStarted)
                seasoningInstance.setPaused(true);
            return; // Makes sure game isn't paused before anything happens
        }
        else
        {
            if (seasoningStarted)
                seasoningInstance.setPaused(false);
        }


        //If A is pressed and next key is A: move shaker, shake count increments, and seasoning falls
        if (Input.GetKeyDown(KeyCode.A) && !nextKeyIsD)
        {
            StartSeasoningSfx();
            shaker.position = new Vector2(startPos.x - movementOffset, startPos.y);
            nextKeyIsD = true;
            shakeCount++;
            particleSystem.Play();
        }

        //If D is pressed and next key is D: move shaker, shake count increments, and seasoning falls
        else if (Input.GetKeyDown(KeyCode.D) && nextKeyIsD)
        {
            shaker.position = new Vector2(startPos.x + movementOffset, startPos.y);
            nextKeyIsD = false;
            shakeCount++;
            particleSystem.Play();
        }

        //Checks if enough shakes have been done, if so transition to next step
        if (shakeCount >= totalShakes)
        {
            StopSeasoningSfx();
            CookingManager.instance.Transition();
            controlsText.SetActive(false);
            gameObject.SetActive(false);
            particleSystem.gameObject.SetActive(false);
        }
    }
    private void StartSeasoningSfx()
    {
        if (seasoningStarted) return;
        if (string.IsNullOrEmpty(seasoningLoopEvent)) return;

        seasoningInstance = FMODUnity.RuntimeManager.CreateInstance(seasoningLoopEvent);
        seasoningInstance.start();
        seasoningStarted = true;
    }

    private void StopSeasoningSfx()
    {
        if (!seasoningStarted) return;

        seasoningInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        seasoningInstance.release();
        seasoningStarted= false;
    }

    private void OnDisable()
    {
        StopSeasoningSfx(); // if shaker gets disabled, stop the loop
    }
}
