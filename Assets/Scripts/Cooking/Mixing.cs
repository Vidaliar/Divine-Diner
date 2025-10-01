using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Mixing : MonoBehaviour
{
    [SerializeField] GameObject controlsText;
    // [SerializeField] Transform shaker;
    [SerializeField] int totalMixes = 5;
    [SerializeField] Vector2 center = new Vector2(0,0);

    Vector2 prevPos;

    int mixCount = 0;
    float currentMix = 0;   //Once this reaches 2pi, mixCount++ and currentMix = 0
    const float fullMixValue = 2 * Mathf.PI;

    void Start()
    {
        controlsText.SetActive(true);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            prevPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(0))
            {
                Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CalculateRotation(pos);
                Debug.Log(currentMix + " is current mix");
            }

        if (currentMix >= fullMixValue || currentMix <= -fullMixValue)
        {
            mixCount++;
            currentMix = 0;
            Debug.Log("MIX COUNT UPDATED TO " + mixCount);
        }

        if (mixCount >= totalMixes)
            {
                CookingManager.instance.Transition();
                controlsText.SetActive(false);
                gameObject.SetActive(false);
            }
    }

    /*
    while mousebutton, detect if mouse is going in a circle


    */
    void CalculateRotation(Vector2 currentPos)
    {
        /*
        Can do 2 ways:
        1. Calculate Vector2.right to Vector2(prev -> center) and Vector2.right to Vector2(current -> center)
           then take the difference in radians as the change
        or
        2. Calculate Vector2(prev -> center) to Vector2(current -> center)
        */

        //Try 1 below
        float deltaX = currentPos.normalized.x - prevPos.normalized.x;
        float deltaY = currentPos.normalized.y - prevPos.normalized.y;

        float centerCurrentX = currentPos.normalized.x - Vector2.right.x;
        float centerCurrentY = currentPos.normalized.y - Vector2.right.y;

        float centerPrevX = prevPos.normalized.x - Vector2.right.x;
        float centerPrevY = prevPos.normalized.y - Vector2.right.y;

        // float deltaX = centerCurrentX - centerPrevX;
        // float deltaY = centerCurrentY - centerPrevY;

        float radians = Mathf.Atan2(deltaY, deltaX);
        
        Debug.Log(radians + " is the radians with deltaX: " + deltaX + " and deltaY: " + deltaY);

        currentMix += radians;
        prevPos = currentPos;
    }
}
