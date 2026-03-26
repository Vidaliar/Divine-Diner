using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

using FMODUnity;
using FMOD.Studio;

public class FryingPan : MonoBehaviour
{
    [SerializeField] float timeSec = 5.5f;  // Time until can flip
    [SerializeField] GameObject spaceText;
    [SerializeField] Slider progressBar;
    [SerializeField] GameObject flipObj;    // The object or food to be flipped

    Vector2 upperPos;   // The position for the top of the flip
    float timer;        // Keeps track of time passed
    Vector2 startPos;   // Holds the starting position of the flipping object
    bool flipping = false;  // Does or doesn't allow flipObj to be flipped
    int numFlips = 0;

    bool inPause = false;
    public CookingManager cManager;

    [Header("FMOD SFX")]
    [FMODUnity.EventRef]
    [SerializeField] private string sizzleLoopEvent;   // example: event:/Sound Effects/Sizzle (Frying Something)

    [FMODUnity.EventRef]
    [SerializeField] private string flipOneShotEvent;  // optional

    private EventInstance sizzleInstance;
    private bool sizzleStarted = false;

    void Start()
    {
        startPos = flipObj.transform.position;
        upperPos = new Vector2(startPos.x, startPos.y + 5);

        StartSizzle();

        progressBar.minValue = 0;
        progressBar.maxValue = timeSec;
        progressBar.value = 0;
        progressBar.gameObject.SetActive(true);
    }

    void Update()
    {
        inPause = cManager.inPause;

        // Pause handling for FMOD loop
        if (inPause)
        {
            if (sizzleStarted) sizzleInstance.setPaused(true);
            return; // Makes sure game isn't paused before anything happens
        }
        else
        {
            if (sizzleStarted) sizzleInstance.setPaused(false);
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("p");
            Flip();
        }

        // Keep 3D attributes updated (safe even if your event is 2D)
        if (sizzleStarted)
        {
            sizzleInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        }

        // Checks if the timer to flip has gone up enough
        if (timer >= timeSec)
        {
            spaceText.SetActive(true);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PlayFlipOneShot();

                timer = 0;
                progressBar.value = 0;
                spaceText.SetActive(false);
                flipping = true;
                numFlips++;
            }
        }

        if (!flipping)
        {
            timer += Time.deltaTime;
            progressBar.value = timer;
        }

        // if (flipping) Flip();

        // If it's been flipped twice and done flipping, transition to next step and don't receive input
        if (numFlips >= 2 && !flipping)
        {
            StopSizzle();

            progressBar.gameObject.SetActive(false);
            CookingManager.instance.Transition();
            this.gameObject.SetActive(false);
        }
    }

    // Fractionally moves up and rotates the flippable object like it's being flipped
    void Flip()
    {
        flipObj.GetComponent<Animator>().Play("SteakFlip");
        // if (timer > timeSec / 2f)
        // {
        //     flipping = false;
        //     timer = 0;
        // }
        // else
        // {
        //     timer += Time.deltaTime;
        //     flipObj.transform.position = new Vector2(
        //         startPos.x,
        //         Mathf.Sin(timer / (timeSec / 2) * 3.14f) * upperPos.y + startPos.y
        //     );
        //     flipObj.transform.Rotate(0, 0, (180 / (timeSec / 2)) * Time.deltaTime);
        // }
    }

    private void StartSizzle()
    {
        if (sizzleStarted) return;
        if (string.IsNullOrEmpty(sizzleLoopEvent)) return;

        sizzleInstance = RuntimeManager.CreateInstance(sizzleLoopEvent);
        sizzleInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        sizzleInstance.start();
        sizzleStarted = true;
    }

    private void StopSizzle()
    {
        if (!sizzleStarted) return;

        if (sizzleInstance.isValid())
        {
            sizzleInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            sizzleInstance.release();
        }

        sizzleStarted = false;
    }

    private void PlayFlipOneShot()
    {
        if (string.IsNullOrEmpty(flipOneShotEvent)) return;
        RuntimeManager.PlayOneShot(flipOneShotEvent, transform.position);
    }

    private void OnDisable()
    {
        // Safety: if this object gets disabled unexpectedly, stop the loop
        StopSizzle();
    }
}
