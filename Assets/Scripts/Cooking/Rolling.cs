using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

using FMODUnity;
using FMOD.Studio;

public class Rolling : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] Transform rollingPin;
    [SerializeField] Transform dough;

    [Header("Rolling Info")]
    [SerializeField] int totalRolls = 3;
    [SerializeField] float rollSpeed = 3f;

    [Header("Dough End Scale")]
    //The dough might already be scaled, so the end scale will be og scale + size diff
    [SerializeField] float xSizeDiff = 1.5f;
    [SerializeField] float ySizeDiff = 1f;

    [Header("Testing")]
    [SerializeField] bool speedLimit = true;
    [SerializeField] bool moveBased = false;
    [SerializeField] bool dynamicSizeBasing = false;

    int rolls = 0;

    //The values to add to the scale for each roll
    float xSizeFrac;
    float ySizeFrac;

    bool canRoll = false;
    bool nextIsRight = true;
    Vector2 pinDirection = Vector2.up;
    
    Vector2 minDoughBounds;
    Vector2 maxDoughBounds;

    Vector2 startDoughSize;

    Vector2 startPinPos;
    Vector2 prevPinPos;

    bool interactable = true;

    bool inPause = false;

    public CookingManager cManager; //Not needed since CookingManager is a singleton but okie

    [Header("FMOD SFX")]
    [FMODUnity.EventRef]
    [SerializeField] private string rollingLoopEvent; // example: event:/Sound Effects/Rolling Dough (New)

    private EventInstance rollingInstance;
    private bool rollingSfxStarted = false;
    void Start()
    {
        xSizeFrac = (xSizeDiff-dough.localScale.x) / totalRolls;
        ySizeFrac = (ySizeDiff-dough.localScale.y) / totalRolls;
        UpdateColliderBounds();
        //Roll distance to determine how many rolls? Or actually probably if it's at it's final size
        startPinPos = prevPinPos = rollingPin.position;
        startDoughSize = new Vector2(maxDoughBounds.x - minDoughBounds.x, maxDoughBounds.y - minDoughBounds.y);
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
        //Maybe use different data type to hold all maxY, minY, maxX, and minX, maybe make struct? Maybe Bounds

        // if(moveBased) UpdateColliderBounds();
        UpdateColliderBounds(); //Works best within Update but would like to only get bounds when they change
        
        // Bounds doughBounds = dough.gameObject.GetComponent<Collider2D>().bounds;
        // Debug.Log("IN UPDATE Min:"+doughBounds.min.x + ","+doughBounds.min.y + " | Max:"+doughBounds.max.x+","+doughBounds.max.y);

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

        if (Input.GetMouseButton(0) && canRoll && interactable)
        {
            Roll();
            CheckPinPosition();
        }

        //Transition to next step if player has rolled enough
        if(rolls >= totalRolls && !moveBased && pinDirection == Vector2.up)
        {
            StartHorizontal();
        }

        //Checks if the player is done rolling when it isn't move based
        else if(rolls >= totalRolls && !moveBased && pinDirection == Vector2.right)
        {
            StopRollingSfx();
            CookingManager.instance.Transition();
            this.gameObject.SetActive(false);
        }

        //Checks for the player has rolled enough vertically if it's move based
        else if(moveBased && dough.localScale.y >= ySizeDiff && pinDirection == Vector2.up)
        {
            StartHorizontal();
            //Base it off of sizeDiff or totalRolls & size
            //sizeDiff would be something like - if(localScale._ >= sizeDiff_) transition - can transition to hori or next minigame
            //check total distance totalRolls & size ... maybe not
        }

        //Checks if the player is done rolling when it's move based
        else if(moveBased && dough.localScale.x >= xSizeDiff && pinDirection == Vector2.right)
        {
            StopRollingSfx();
            CookingManager.instance.Transition();
            this.gameObject.SetActive(false);
        }
    }

    void UpdateColliderBounds()
    {
        Physics.SyncTransforms();
        Bounds doughBounds = dough.gameObject.GetComponent<Collider2D>().bounds;
        minDoughBounds = new Vector2(doughBounds.center.x - doughBounds.extents.x, doughBounds.center.y - doughBounds.extents.y);
        maxDoughBounds = new Vector2(doughBounds.center.x + doughBounds.extents.x, doughBounds.center.y + doughBounds.extents.y);

        // //Check if doing vertical first messes up hori?
        // minDoughBounds = new Vector2(minDoughBounds.x*dough.localScale.x, minDoughBounds.y*dough.localScale.y);
        // maxDoughBounds = new Vector2(maxDoughBounds.x*dough.localScale.x, maxDoughBounds.y*dough.localScale.y);
        Debug.Log($"FUNCTION Min: ({minDoughBounds.x},{minDoughBounds.y}) and max: ({maxDoughBounds.x},{maxDoughBounds.y})");
        Debug.Log(doughBounds.extents.x);
    }

    void Roll()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Normalized vector to move the pin
        Vector2 mouseDirection = new Vector2(mousePos.x - rollingPin.position.x, mousePos.y - rollingPin.position.y).normalized;

        //Direction of pin to move with mouse
        Vector2 moveDirection = mouseDirection * pinDirection;

        //New position for the rolling pin
        Vector2 newPinPos;
        if(speedLimit) newPinPos = (Vector2)rollingPin.position + (moveDirection * rollSpeed * Time.deltaTime);
        else if(pinDirection == Vector2.right) newPinPos = new Vector2((pinDirection * mousePos).x, rollingPin.position.y);
        else newPinPos = pinDirection * mousePos;

        //Clamped x and y for rolling pin position
        float newX = Mathf.Clamp(newPinPos.x, minDoughBounds.x, maxDoughBounds.x);
        float newY = Mathf.Clamp(newPinPos.y, minDoughBounds.y, maxDoughBounds.y);

        prevPinPos = rollingPin.position;
        rollingPin.position = new Vector2(newX, newY);

        //Scaling dough when move (aka distance) based
        if(moveBased)
        {
            //Scale dough here
            //Maybe move into CheckPinPosition (to be renamed to ScaleDough?)
            //Maybe have moveDistance 

            //Get the length of the dough for the current rolling direction
            float doughLength;
            if(pinDirection == Vector2.up) doughLength = maxDoughBounds.y - minDoughBounds.y;
            else doughLength = maxDoughBounds.x - minDoughBounds.x;

            //Distance the rolling pin moved
            float distance = Vector3.Distance(prevPinPos, rollingPin.position);

            //Fractional distance of pin movement
            float fractDist;
            if(dynamicSizeBasing) fractDist = distance / doughLength;  //Divided by current length of dough? Yesm?
            else if(pinDirection == Vector2.up) fractDist = distance / startDoughSize.y;
            else fractDist = distance / startDoughSize.x;

            //Actual scaling
            dough.localScale += new Vector3(xSizeFrac*pinDirection.x, ySizeFrac*pinDirection.y, 0) * fractDist /2;
        }
    }


    //Checks if pin hit the correct side, and if so, increments rolls and expand dough
    void CheckPinPosition()
    {
        if(pinDirection == Vector2.up)
        {
            if((rollingPin.position.y <= minDoughBounds.y && !nextIsRight) || (rollingPin.position.y >= maxDoughBounds.y && nextIsRight))
            {
                nextIsRight = !nextIsRight;
                rolls++;
                dough.localScale = new Vector2(dough.transform.localScale.x, dough.transform.localScale.y + ySizeFrac);
                UpdateColliderBounds();
                Debug.Log($"After func - min: ({minDoughBounds.x},{minDoughBounds.y}) and max: ({maxDoughBounds.x},{maxDoughBounds.y})");
            }   
        }
        else if((rollingPin.position.x <= minDoughBounds.x && !nextIsRight) || (rollingPin.position.x >= maxDoughBounds.x && nextIsRight))
        {
            nextIsRight = !nextIsRight;
            rolls++;
            dough.localScale = new Vector2(dough.transform.localScale.x + xSizeFrac, dough.transform.localScale.y);
            UpdateColliderBounds();
            Debug.Log($"After func - min: ({minDoughBounds.x},{minDoughBounds.y}) and max: ({maxDoughBounds.x},{maxDoughBounds.y})");
        }
    }

    void StartHorizontal()
    {
        interactable = false;
        
        //Rotate pin
        Animator animator = rollingPin.gameObject.GetComponent<Animator>();
        animator.Play("RollingPinTransition");

        //Center pin
        StartCoroutine(MovePinToStartPos());

        pinDirection = Vector2.right;
        rolls = 0;
    }

    IEnumerator MovePinToStartPos()
    {
        float moveDist = (rollingPin.position.y - startPinPos.y) / 100;
        while(true)
        {
            if(rollingPin.position.y - startPinPos.y <= 0.1)
            {
                rollingPin.position = startPinPos;
                interactable = true;
                break;
            }

            rollingPin.position = new Vector2(startPinPos.x, rollingPin.position.y - moveDist);
            yield return new WaitForSeconds(0.01f);
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
