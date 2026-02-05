using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Mixing : MonoBehaviour
{
    [SerializeField] GameObject controlsText;
    [SerializeField] int totalMixes = 7;
    [SerializeField] Vector2 center = new Vector2(0, 0);
    [SerializeField] Slider progressBar;
    [SerializeField] Animator spoonAnim;
    [SerializeField] float spoonAnimSpeed = 2;

    //Used to calculate the radial difference of the mouse each frame
    Vector2 prevPos;

    int mixCount = 0;

    //Tracks the progress of current mix, once this reaches pi, mixCount++ and currentMix = 0
    float currentMix = 0;
    float prevMix = 0;
    const float fullMixValue = Mathf.PI;

    bool canMix = false;

    bool goingClockwise = true;

    bool inPause = false;
    public CookingManager cManager;
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
        inPause = cManager.inPause;     // can be changed to be CookingManager.instance.inPause;
        if (inPause) return; // Makes sure game isn't paused before anything happens

        //Sets previous position to initially be the mouse position on the first click
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            prevPos = new Vector2(pos.x - center.x, pos.y - center.y).normalized;
            canMix = true;
        }

        //Gets the current mouse position and calculates the angle difference between the current and prev
        if (Input.GetMouseButton(0) && canMix)
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(Input.mousePosition);
            Debug.Log(pos);
            CalculateRotation(pos);
            CalculateSpeed();

            //Updates the progress bar
            progressBar.value = mixCount * fullMixValue + Mathf.Clamp(currentMix, 0, fullMixValue);
        }

        //Stops the spoon animation if the player lets go of the button
        if(Input.GetMouseButtonUp(0)) {spoonAnim.speed = 0;}

        //Checks if the current mix has done a full rotation, updates mix count and current mix = 0
        if (currentMix >= fullMixValue || currentMix <= -fullMixValue)
        {
            mixCount++;
            currentMix = 0;
            prevMix = 0;
        }

        //If there's been enough mixes, transition to next step
        if (mixCount >= totalMixes)
        {
            controlsText.SetActive(false);
            progressBar.gameObject.SetActive(false);
            CookingManager.instance.Transition();
            spoonAnim.speed = 0;
            gameObject.SetActive(false);
        }
    }

    //Honestly not sure if this is the best way to do this math, but it works lol
    //Calculates the angle between the previous and current mouse position, also determines the direction of rotation
    //Bug: This way technically allows the player to move the mouse up and down and eventually get it mixed
    void CalculateRotation(Vector2 currentPos)
    {
        float currMinCenterx = currentPos.x - center.x;
        float currMinCentery = currentPos.y - center.y;
        Vector2 currNormalized = new Vector2(currMinCenterx, currMinCentery).normalized;
        // Debug.Log(prevPos + " is prev and curr is " + currNormalized);

        float centerCurrentX = 1 - currNormalized.x;
        float centerCurrentY = currNormalized.y;

        float centerPrevX = 1 - prevPos.x;
        float centerPrevY = prevPos.y;

        // Debug.Log("Current pos is " + centerCurrentX +","+centerCurrentY + " and prev is " + centerPrevX + "," + centerPrevY);

        float currentRadians = Mathf.Atan2(centerCurrentY, centerCurrentX);
        float prevRadians = Mathf.Atan2(centerPrevY, centerPrevX);
        // Debug.Log("Current rads: " + currentRadians + ", and prev are: " + prevRadians);

        float radians = Mathf.Abs(currentRadians) - Mathf.Abs(prevRadians);
        // Debug.Log("Radians" + radians);
       
        prevMix = currentMix;
        currentMix += Mathf.Abs(radians);
        // Debug.Log("currentMix: " + currentMix + " and prev: " + prevMix);
        prevPos = currNormalized;

        //Checking mixing direction
        CheckMixDirection(prevRadians, currentRadians);
    }

    void CheckMixDirection(float prevRadians, float currentRadians)
    {
        if(prevRadians >= 0 && currentRadians < 0)
        {
            if(prevRadians > 1 && !goingClockwise)
            {
                SwitchAnimDirection();
                goingClockwise = true;
                return;
            }
            else if(goingClockwise)
            {
                SwitchAnimDirection();
                goingClockwise = false;
                return;
            }
        }
        else if(prevRadians <= 0 && currentRadians > 0)
        {
            if(prevRadians < -1 && goingClockwise)
            {
                SwitchAnimDirection();
                goingClockwise = false;
                return;
            }
            else if(!goingClockwise)
            {
                SwitchAnimDirection();
                goingClockwise = true;
                return;
            }
        }
        else
        {
            if(prevRadians < currentRadians && !goingClockwise)
            {
                SwitchAnimDirection();
                goingClockwise = true;
                return;
            }
            else if(prevRadians > currentRadians && goingClockwise)
            {
                SwitchAnimDirection();
                goingClockwise = false;
                return;
            }
        }

        // Debug.Log("Going clockwise is " + goingClockwise);
    }

    void CalculateSpeed()   //Maybe rename to be SpoonAnimation? 
    {
        //Play speed of 1 = 2s animation = 2s to make 1 full rotation, so fullMixValue / Mathf.Abs(prevMix-currentMix)
        //animState = spoonAnim.GetCurrentAnimatorState();
        float fractionOfMix = (currentMix-prevMix) / fullMixValue;

        float animSpeed = spoonAnimSpeed*fractionOfMix / Time.deltaTime;
        spoonAnim.speed = animSpeed;
        // Debug.Log(animSpeed);

        //For 1s = 1.57 currentMix
        //So 1s * Time.deltaTime = 1.57 * Time.deltaTime
    }

    void SwitchAnimDirection()
    {
        float currAnimTime = spoonAnim.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;

        if(goingClockwise)
        {
            spoonAnim.Play("SpoonAnimation", 0, Mathf.Abs(1-currAnimTime));
        }
        else
        {
            spoonAnim.Play("ReversedSpoonAnimation", 0, Mathf.Abs(1-currAnimTime));
        }
        // Debug.Log("Anim time: "+currAnimtime);
    }
}
