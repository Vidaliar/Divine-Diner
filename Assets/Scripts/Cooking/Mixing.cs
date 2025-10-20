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

    //Honestly not sure if this is the best way to do this math, but it works lol
    //Calculates the angle between the previous and current mouse position
    //This way technically allows the player to move the mouse up and down and eventually get it mixed
    void CalculateRotation(Vector2 currentPos)
    {
        float centerCurrentX = 1- currentPos.normalized.x;
        float centerCurrentY = currentPos.normalized.y;

        float centerPrevX = 1 - prevPos.normalized.x;
        float centerPrevY = prevPos.normalized.y;

        float currentRadians = Mathf.Atan2(centerCurrentY, centerCurrentX);
        float prevRadians = Mathf.Atan2(centerPrevY, centerPrevX);

        float radians = currentRadians - prevRadians;

        currentMix += Mathf.Abs(radians);
        prevPos = currentPos;
    }
}
