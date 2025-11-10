using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Saute : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] Transform pan;
    [SerializeField] Slider progressBar;
    [SerializeField] GameObject instructions;
    [SerializeField] GameObject instuctArrows;

    [Header("Numerical values")]
    [SerializeField] int maxMoves = 16;
    [SerializeField] float maxTime = 4f;
    [SerializeField] int numSections = 4;
    // [SerializeField] float sauteSpeed;

    int moves = 0;  //Not sure what to call these rn lol, the back and forth are the moves
    int totalMoves = 0;
    int moveSection;

    float minX;
    float maxX;

    float distance = 2f;
    float currDistance = 0;

    float timer = 0f;
    float totalTime = 0f;
    float timeSection;

    Vector2 prevPosition;

    bool canSaute = false;

    // Start is called before the first frame update
    void Start()
    {
        maxX = pan.position.x + 1f;
        minX = maxX - 2f;

        timeSection = maxTime / numSections;
        moveSection = maxMoves / numSections;

        progressBar.minValue = 0;
        progressBar.maxValue = maxTime;
        progressBar.value = 0;
        progressBar.gameObject.SetActive(true);

        // instructions.SetActive(true);
        // instuctArrows.SetActive(true);

        prevPosition = pan.position;
    }

    // Update is called once per frame
    void Update()
    {
        //Allows the player to start sauteing after the any previous prep
        if (Input.GetMouseButtonDown(0))
        {
            canSaute = true;
        }
        if (Input.GetMouseButton(0) && canSaute && timer >= timeSection)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //Normalized vector to move the pin
            // Vector2 direction = new Vector2(mousePos.x - pan.position.x, mousePos.y - pan.position.y).normalized;

            //The x value to set the pin x value as
            // float newX = pan.position.x + (direction.x * sauteSpeed * Time.deltaTime);
            // float clampX = Mathf.Clamp(newX, minX, maxX);
            // float newX = pan.position.x + (direction.x * sauteSpeed * Time.deltaTime);
            float clampX = Mathf.Clamp(mousePos.x, minX, maxX);

            pan.position = new Vector2(clampX, pan.position.y);

            currDistance += Mathf.Abs(prevPosition.x - pan.position.x);
            prevPosition = pan.position;

            if (currDistance >= distance)
            {
                moves++;
                currDistance = 0;
            }
        }

        if (timer >= timeSection)
        {
            instructions.SetActive(true);
            instuctArrows.SetActive(true);
        }
        else
        {
            instructions.SetActive(false);
            instuctArrows.SetActive(false);
        }

        // Debug.Log("Timer: " + timer);
        if (timer < timeSection)
        {
            timer += Time.deltaTime;
            totalTime += Time.deltaTime;
            progressBar.value = totalTime;
        }

        if(moves == numSections)
        {
            totalMoves += moves;
            timer = 0;
            moves = 0;
        }

        if(totalMoves >= maxMoves)
        {
            CookingManager.instance.Transition();
            progressBar.gameObject.SetActive(false);
            instructions.SetActive(false);
            instuctArrows.SetActive(false);
            this.gameObject.SetActive(false);
        }
    }
}
