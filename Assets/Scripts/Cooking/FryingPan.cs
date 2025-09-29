using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FryingPan : MonoBehaviour
{
    [SerializeField] float timeSec = 5.5f;  //Time until can flip
    [SerializeField] GameObject spaceText;
    [SerializeField] GameObject flipObj;    //The object or food to be flipped

    Vector2 upperPos;   //The position for the top of the flip
    float timer;
    Vector2 startPos;
    bool flipping = false;  //Does or doesn't allow flipObj to be flipped
    int numFlips = 0;
    
    void Start()
    {
        startPos = flipObj.transform.position;
        upperPos = new Vector2(startPos.x, startPos.y + 5);
    }

    void Update()
    {
        //Checks if the timer to flip has gone up enough
        if (timer >= timeSec)
        {
            spaceText.SetActive(true);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                timer = 0;
                spaceText.SetActive(false);
                flipping = true;
                numFlips++;
            }
        }

        timer += Time.deltaTime;
        if (flipping) Flip();

        //If it's been flipped twice and done flipping, transition to next step and don't receive input
        if (numFlips >= 2 && !flipping)
        {
            CookingManager.instance.Transition();
            this.gameObject.SetActive(false);
        }
    }

    //Fractionally moves up and rotates the flippable object like it's being flipped
    void Flip()
    {
        if (timer > timeSec/2f)
        {
            flipping = false;
        }
        else
        {
            flipObj.transform.position = new Vector2(startPos.x, Mathf.Sin(timer / (timeSec/2) * 3.14f) * upperPos.y + startPos.y);
            flipObj.transform.Rotate(0, 0, (180f/(timeSec/2))*Time.deltaTime);
        }
    }
}
