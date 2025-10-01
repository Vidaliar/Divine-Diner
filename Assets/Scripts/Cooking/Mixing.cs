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
        if (Input.GetMouseButton(0))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateRotation(pos);
            Debug.Log(currentMix + " is current mix");
        }

        if (currentMix >= fullMixValue)
        {
            mixCount++;
            currentMix = 0;
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
        float deltaX = currentPos.x - prevPos.x;
        float deltaY = currentPos.y - prevPos.y;

        float radians = Mathf.Atan2(deltaX, deltaY);
        Debug.Log(radians + " is the radians");

        currentMix += radians;
    }
}
