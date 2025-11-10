using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grater : MonoBehaviour
{
    [SerializeField] GameObject grateObj;
    [SerializeField] GameObject grater;
    [SerializeField] GameObject grateInstructions;
    [SerializeField] int totalGrates;
    [SerializeField] float grateSpeed = 7;
    [SerializeField] float totalDistance = 25;

    bool grating;
    int gratingCount = 0;
    //Vector2 grateObjStartPos;

    Vector2 grateDirection;

    //The min and max determine if the grated object hit the top or bottom of the grater
    float minY;
    float maxY;

    float currDistance = 0;

    bool topSide = false;   //A bool to track which side needs hit 
    void Start()
    {
        //grateObjStartPos = grateObj.transform.position;

        //Grater should have a collider2D
        maxY = grater.GetComponent<Collider2D>().bounds.max.y;
        minY = grater.GetComponent<Collider2D>().bounds.min.y;

        //Ensures the grater isn't in front of the cheese and interfering with raycasting
        grater.transform.localPosition = new Vector3(grater.transform.localPosition.x, grater.transform.localPosition.y, 1);

        //The direction along the grater
        grateDirection = new Vector2(0.23f, 0.97f);

        grateInstructions.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Checks if the player clicked on the grated object, if true -> start grating
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
            //Vector to move the grateObj towards the mouse
            Vector2 direction = new Vector2(pos.x - grateObj.transform.position.x, pos.y - grateObj.transform.position.y);

            //Normalized direction vector
            Vector2 normalizedDir = direction.normalized;

            //Minimum length of the mouse to grateObj to avoid a shaky cheese
            float dirLength = Mathf.Min(direction.magnitude, 1);

            //Set the grate object y value (just added the * grateDirection.y)
            float newY = grateObj.transform.position.y + (normalizedDir.y * grateDirection.y * (grateSpeed * dirLength) * Time.deltaTime);
            float clampY = Mathf.Clamp(newY, minY, maxY);

            //newX is y times the slope of the grateDirection
            float newX = (grateDirection.x / grateDirection.y) * clampY;

            //Gets the distance of the current and next position
            currDistance += Vector3.Distance(grateObj.transform.localPosition, new Vector3(newX, clampY, 0));

            //Updates the objects location
            grateObj.transform.localPosition = new Vector2(newX, clampY);
            
            //Checks if the grate object is at an end and checks if it's the correct side, if so -> count++ and flip topSide
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
        // if (gratingCount >= totalGrates)
        // {
        //     CookingManager.instance.Transition();
        //     gameObject.SetActive(false);
        // }

        //Checks if the distance grated is enough
        if (currDistance >= totalDistance)
        {
            CookingManager.instance.Transition();
            grateInstructions.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
