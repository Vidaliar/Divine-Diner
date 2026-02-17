using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Rolling : MonoBehaviour
{
    [SerializeField] Transform rollingPin;
    [SerializeField] GameObject dough;
    [SerializeField] int totalRolls = 3;
    [SerializeField] float rollSpeed = 3f;

    //The dough might already be scaled, so the end scale will be og scale + size diff
    [SerializeField] float xSizeDiff = 5f;
    [SerializeField] float ySizeDiff = 2f;

    int rolls = 0;
    
    //The values to add to the scale for each roll
    float xSizeFrac;
    float ySizeFrac;

    bool canRoll = false;
    bool nextIsRight = true;

    bool inPause = false;

    Bounds doughBounds;

    public CookingManager cManager;
    void Start()
    {
        xSizeFrac = xSizeDiff / totalRolls;
        ySizeFrac = ySizeDiff / totalRolls;
        UpdateColliderBounds();
    }

    void Update()
    {
        inPause = cManager.inPause;
        if (inPause) return; // Makes sure game isn't paused before anything happens

        //Dough collider bounds
        //Maybe use different data type to hold all maxY, minY, maxX, and minX, maybe make struct? Maybe Bounds
        // float doughMinBound = dough.GetComponent<Collider2D>().bounds.min.x;
        // float doughMaxBound = dough.GetComponent<Collider2D>().bounds.max.x;

        //Allows the player to start rolling after the any previous prep
        if (Input.GetMouseButtonDown(0))
        {
            canRoll = true;
        }

        if (Input.GetMouseButton(0) && canRoll)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //Normalized vector to move the pin
            Vector2 direction = new Vector2(mousePos.x - rollingPin.position.x, mousePos.y - rollingPin.position.y).normalized;

            //The x value to set the pin x value as
            float newX = rollingPin.position.x + (direction.x * rollSpeed * Time.deltaTime);
            float clampX = Mathf.Clamp(newX, doughMinBound, doughMaxBound);

            rollingPin.position = new Vector2(clampX, rollingPin.position.y);

            /*
            Needs to be changed so that it takes rolls on both sides to have the dough grow
            */
            
            //Checks if pin hit the correct side, and if so, increments rolls and expand dough
            if ((clampX >= doughMaxBound && nextIsRight) || (clampX <= doughMinBound && !nextIsRight))
            {
                nextIsRight = !nextIsRight;
                rolls++;
                dough.transform.localScale = new Vector2(dough.transform.localScale.x + xSizeFrac, dough.transform.localScale.y + ySizeFrac);
            }
        }
        
        //Transition to next step if player has rolled enough
        if(rolls >= totalRolls)
        {
            CookingManager.instance.Transition();
            this.gameObject.SetActive(false);
        }
    }

    void UpdateColliderBounds()
    {
        doughBounds = dough.GetComponent<Collider2D>().bounds;
    }
}
