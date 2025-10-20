using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grater : MonoBehaviour
{
    [SerializeField] GameObject grateObj;
    [SerializeField] GameObject grater;
    [SerializeField] int totalGrates;
    [SerializeField] float grateSpeed = 7;

    bool grating;
    int gratingCount = 0;
    Vector2 grateObjStartPos;

    Vector2 grateDirection;

    //The min and max determine if the grated object hit the top or bottom of the grater
    float minY;
    float maxY;

    bool topSide = false;   //A bool to track which side needs hit 
    void Start()
    {
        grateObjStartPos = grateObj.transform.position;

        //Y values need to be changed for when grater + cheese assets are made
        //Maybe add a collider and that can be used to reference the size instead of the sprite
        // minY = grater.transform.position.y - grater.transform.localScale.y / 2;
        // maxY = grater.transform.position.y + grater.transform.localScale.y / 2;

        maxY = 2.52f;
        minY = -2.25f;

        grateDirection = new Vector2(0.24f, 0.97f);
        //grateDirection = new Vector2(1.18, 4.77).normalized;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Checks if the player clicked on the grated object, if true start grating
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.up, 0.1f);
            if (hit.collider != null)
            {
                GameObject otherObj = hit.collider.gameObject;
                if (otherObj == grateObj) grating = true;
            }
        }

        //Does the grating
        if (Input.GetMouseButton(0) && grating)
        {
            //Normalized vector to move the pin
            Vector2 direction = new Vector2(pos.x - grateObj.transform.position.x, pos.y - grateObj.transform.position.y).normalized;

            //Set the grate object y value (just added the * grateDirection.y)
            float newY = grateObj.transform.position.y + (direction.y * grateDirection.y * grateSpeed * Time.deltaTime);
            float clampY = Mathf.Clamp(newY, minY, maxY);

            //newX is all new
            float newX = grateObj.transform.position.x + (direction.x * grateDirection.x * grateSpeed * Time.deltaTime);

            //original was grateObj.transform.position = new Vector2(grateObjStartPos.x, clampY);

            grateObj.transform.position = new Vector2(newX, clampY);
            
            //Checks if the grate object is at an end and checks if it's the correct side, if so count++
            if (grateObj.transform.position.y <= minY && !topSide)
            {
                gratingCount++;
                topSide = true;
            }
            if (grateObj.transform.position.y >= maxY && topSide)
            {
                gratingCount++;
                topSide = false;
            }
        }

        //Stops grateObj movement
        if (Input.GetMouseButtonUp(0))
        {
            grating = false;
        }

        //Checks if grateObj has been grated enough and moves on if true
        if (gratingCount >= totalGrates)
        {
            CookingManager.instance.Transition();
            gameObject.SetActive(false);
        }
    }
}
