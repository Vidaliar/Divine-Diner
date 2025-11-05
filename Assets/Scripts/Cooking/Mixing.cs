using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Mixing : MonoBehaviour
{
    [SerializeField] GameObject controlsText;
    [SerializeField] int totalMixes = 5;
    [SerializeField] Vector2 center = new Vector2(0, 0);
    [SerializeField] Slider progressBar;

    //Used to calculate the radial difference of the mouse each frame
    Vector2 prevPos;

    int mixCount = 0;

    //Tracks the progress of current mix, once this reaches pi, mixCount++ and currentMix = 0
    float currentMix = 0;
    const float fullMixValue = Mathf.PI;

    void Start()
    {
        controlsText.SetActive(true);

        progressBar.minValue = 0;
        progressBar.maxValue = totalMixes * fullMixValue;
        progressBar.value = 0;
        progressBar.gameObject.SetActive(true);

        center = transform.position;
    }

    void Update()
    {
        //Sets previous position to initially be the mouse position on the first click
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            prevPos = new Vector2(pos.x - center.x, pos.y-center.y).normalized;
        }

        //Gets the current mouse position and calculates the angle difference between the current and prev
        if (Input.GetMouseButton(0))
            {
                Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CalculateRotation(pos);

                //Updates the progress bar
                progressBar.value = mixCount * fullMixValue + Mathf.Clamp(currentMix, 0, fullMixValue);
            }

        //Checks if the current mix has done a full rotation, updates mix count and current mix = 0
        if (currentMix >= fullMixValue || currentMix <= -fullMixValue)
        {
            mixCount++;
            currentMix = 0;
        }

        //If there's been enough mixes, transition to next step
        if (mixCount >= totalMixes)
        {
            controlsText.SetActive(false);
            progressBar.gameObject.SetActive(false);
            CookingManager.instance.Transition();
            gameObject.SetActive(false);
        }
    }

    //Honestly not sure if this is the best way to do this math, but it works lol
    //Calculates the angle between the previous and current mouse position
    //Bug: This way technically allows the player to move the mouse up and down and eventually get it mixed
    void CalculateRotation(Vector2 currentPos)
    {
        float currMinCenterx = currentPos.x - center.x;
        float currMinCentery = currentPos.y - center.y;
        Vector2 currNormalized = new Vector2(currMinCenterx, currMinCentery).normalized;

        float centerCurrentX = 1 - currNormalized.x;
        float centerCurrentY = currNormalized.y;

        float centerPrevX = 1 - prevPos.normalized.x;
        float centerPrevY = prevPos.normalized.y;

        float currentRadians = Mathf.Atan2(centerCurrentY, centerCurrentX);
        float prevRadians = Mathf.Atan2(centerPrevY, centerPrevX);

        float radians = Mathf.Abs(currentRadians) - Mathf.Abs(prevRadians);
       
        currentMix += Mathf.Abs(radians);
        prevPos = currentPos;
    }
}
