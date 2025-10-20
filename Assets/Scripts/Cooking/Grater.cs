using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grater : MonoBehaviour
{
    [SerializeField] GameObject grateObj;
    [SerializeField] GameObject grater;
    [SerializeField] int totalGrates;

    bool grating;
    int gratingCount = 0;
    Vector2 grateObjStartPos;
    float minY;
    float maxY;
    bool topSide = false;   //A bool to track which side needs hit 
    void Start()
    {
        grateObjStartPos = grateObj.transform.position;

        //Y values need to be changed for when grater + cheese assets are made
        minY = grater.transform.position.y - grater.transform.localScale.y / 2;
        maxY = grater.transform.position.y + grater.transform.localScale.y / 2;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.up, 0.1f);
            if (hit.collider != null)
            {
                Debug.Log("Hit is " + hit.collider.gameObject.name);
                GameObject otherObj = hit.collider.gameObject;
                if (otherObj == grateObj) grating = true;
            }
        }
        if (Input.GetMouseButton(0) && grating)
        {
            grateObj.transform.position = new Vector2(grateObjStartPos.x, Mathf.Clamp(pos.y, minY, maxY));
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

        //Checks if grateObj has been grated enough and moves on
        if (gratingCount >= totalGrates)
        {
            CookingManager.instance.Transition();
            gameObject.SetActive(false);
        }
    }
}
