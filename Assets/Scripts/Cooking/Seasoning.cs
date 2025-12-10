using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Seasoning : MonoBehaviour
{
    [SerializeField] GameObject controlsText;
    [SerializeField] Transform shaker;
    [SerializeField] ParticleSystem particleSystem;    //Currently used to show the seasoning from the shaker
    [SerializeField] float movementOffset = 1f;     //How much the shaker should move back and forth
    [SerializeField] int totalShakes = 10;

    Vector2 startPos;
    
    int shakeCount = 0;
    bool nextKeyIsD;    //Keeps track if the next key to press is D, if false it's A

    bool inPause = false;
    public CookingManager cManager;
    void Start()
    {
        controlsText.SetActive(true);
        startPos = shaker.position;
    }

    void Update()
    {
        inPause = cManager.inPause;
        if (inPause) return; // Makes sure game isn't paused before anything happens

        //If A is pressed and next key is A: move shaker, shake count increments, and seasoning falls
        if (Input.GetKeyDown(KeyCode.A) && !nextKeyIsD)
        {
            shaker.position = new Vector2(startPos.x - movementOffset, startPos.y);
            nextKeyIsD = true;
            shakeCount++;
            particleSystem.Play();
        }

        //If D is pressed and next key is D: move shaker, shake count increments, and seasoning falls
        else if (Input.GetKeyDown(KeyCode.D) && nextKeyIsD)
        {
            shaker.position = new Vector2(startPos.x + movementOffset, startPos.y);
            nextKeyIsD = false;
            shakeCount++;
            particleSystem.Play();
        }

        //Checks if enough shakes have been done, if so transition to next step
        if (shakeCount >= totalShakes)
        {
            CookingManager.instance.Transition();
            controlsText.SetActive(false);
            gameObject.SetActive(false);
            particleSystem.gameObject.SetActive(false);
        }
    }
}
