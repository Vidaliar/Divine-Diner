using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastTestScript : MonoBehaviour
{
    float mouseX;
    float mouseY;
    Camera cam;
    LineRenderer line;
    Vector2 lastLinePos = new Vector2(-100, -100);
    public float linePointDist = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        line = this.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //Gets the mouse position and turns it to world coords, stored as 'pos'
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        Vector2 pos = cam.ScreenToWorldPoint(Input.mousePosition);
        transform.position = pos;

        //Makes new positions in 'line' while the mouse is held down
        if (Input.GetMouseButton(0))
        {
            if (Vector2.Distance(pos, lastLinePos) > linePointDist)
            {
                //Sets the first position to be where the mouse starts and not 0,0,0 and only happens once
                if (line.GetPosition(0) == new Vector3(0, 0, 0))
                {
                    line.SetPosition(0, pos);
                }

                else
                {
                    line.positionCount++;
                    int index = line.positionCount;
                    line.SetPosition(index - 1, pos);
                    lastLinePos = pos;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        Debug.Log(coll.gameObject.name + " is the collider name");
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        Debug.Log(coll.gameObject.name + " is the collider that exited according to mouse");
    }
}
