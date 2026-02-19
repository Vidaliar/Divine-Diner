using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    bool inPause = false;

    public CookingManager cManager; //Not needed since CookingManager is a singleton but okie
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
        if (inPause) return; // Makes sure game isn't paused before anything happens

        //Dough collider bounds
        //Maybe use different data type to hold all maxY, minY, maxX, and minX, maybe make struct? Maybe Bounds
        // float doughMinBound = dough.GetComponent<Collider2D>().bounds.min.x;
        // float doughMaxBound = dough.GetComponent<Collider2D>().bounds.max.x;
        if(moveBased) UpdateColliderBounds();

        //Allows the player to start rolling after the any previous prep
        if (Input.GetMouseButtonDown(0))
        {
            canRoll = true;
        }

        if (Input.GetMouseButton(0) && canRoll)
        {
            Roll();
            // Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // //Normalized vector to move the pin
            // Vector2 mouseDirection = new Vector2(mousePos.x - rollingPin.position.x, mousePos.y - rollingPin.position.y).normalized;

            // //The x value to set the pin x value as
            // float newX = rollingPin.position.x + (mouseDirection.x * rollSpeed * Time.deltaTime);
            // float clampX = Mathf.Clamp(newX, doughMinBound, doughMaxBound);

            // rollingPin.position = new Vector2(clampX, rollingPin.position.y);

            /*
            Needs to be changed so that it takes rolls on both sides to have the dough grow
            */
            
            //Checks if pin hit the correct side, and if so, increments rolls and expand dough
            // if ((clampX >= doughMaxBound && nextIsRight) || (clampX <= doughMinBound && !nextIsRight))
            // {
            //     nextIsRight = !nextIsRight;
            //     rolls++;
            //     dough.transform.localScale = new Vector2(dough.transform.localScale.x + xSizeFrac, dough.transform.localScale.y + ySizeFrac);
            // }
            CheckPinPosition();
        }
        
        //Transition to next step if player has rolled enough
        if(rolls >= totalRolls && !moveBased && pinDirection == Vector2.up)
        {
            StartHorizontal();
        }
        else if(rolls >= totalRolls && !moveBased && pinDirection == Vector2.right)
        {
            CookingManager.instance.Transition();
            this.gameObject.SetActive(false);
        }
        else if(moveBased && dough.localScale.y >= ySizeDiff && pinDirection == Vector2.up)  //Need to change to check for size instead of num rolls
        {
            StartHorizontal();
            //Base it off of sizeDiff or totalRolls & size
            //sizeDiff would be something like - if(localScale._ >= sizeDiff_) transition - can transition to hori or next minigame
            //check total distance totalRolls & size ... maybe not
        }
        else if(moveBased && dough.localScale.x >= xSizeDiff && pinDirection == Vector2.right)
        {
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
        Debug.Log($"Min: ({minDoughBounds.x},{minDoughBounds.y}) and max: ({maxDoughBounds.x},{maxDoughBounds.y})");
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
    void CheckPinPosition() //Maybe rename to ScaleDough()
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
        //Somewhere needs to check if we switch to hori (done in transition section)
        //Rotate pin
        rollingPin.Rotate(0,0,90);
        //Center pin
        rollingPin.position = startPinPos;
        Debug.Log(startPinPos);
        //Probably disable movement
        //Make animation for rotating and centering? Or just IEnumerator and math it here?

        pinDirection = Vector2.right;
        rolls = 0;
    }
}
