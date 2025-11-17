using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FryingPan : MonoBehaviour
{
    [SerializeField] float timeSec = 5.5f;  //Time until can flip
    [SerializeField] GameObject spaceText;
    [SerializeField] Slider progressBar;
    [SerializeField] GameObject flipObj;    //The object or food to be flipped

    Vector2 upperPos;   //The position for the top of the flip
    float timer;    //Keeps track of time passed
    Vector2 startPos;   //Holds the starting position of the flipping object
    bool flipping = false;  //Does or doesn't allow flipObj to be flipped
    int numFlips = 0;

    void Start()
    {
        startPos = flipObj.transform.position;
        upperPos = new Vector2(startPos.x, startPos.y + 5);
        // AudioManager.Instance.PlaySound("Sizzle");       //Use once stopping audio is solved
        progressBar.minValue = 0;
        progressBar.maxValue = timeSec;
        progressBar.value = 0;
        progressBar.gameObject.SetActive(true);
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

        if (flipping) Flip();

        //If it's been flipped twice and done flipping, transition to next step and don't receive input
        if (numFlips >= 2 && !flipping)
        {
            progressBar.gameObject.SetActive(false);
            CookingManager.instance.Transition();
            this.gameObject.SetActive(false);
        }
    }

    //Fractionally moves up and rotates the flippable object like it's being flipped
    void Flip()
    {
        if (timer > timeSec / 2f)
        {
            flipping = false;
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
            flipObj.transform.position = new Vector2(startPos.x, Mathf.Sin(timer / (timeSec / 2) * 3.14f) * upperPos.y + startPos.y);
            flipObj.transform.Rotate(0, 0, (180 / (timeSec / 2)) * Time.deltaTime);
        }
    }
}
