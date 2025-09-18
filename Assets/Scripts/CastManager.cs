using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastManager : MonoBehaviour
{
    [SerializeField] GameObject castGO;
    [SerializeField] float linePointDist = 0.3f;
    LineRenderer line;
    Vector2 lastLinePos = new Vector2(-100, -100);
    Camera cam;
    int score=0;
    CastSO cast;
    List<GameObject> pathPoints;
    int pointScore = 100;

    void Start()
    {
        line = this.GetComponent<LineRenderer>();
        cam = Camera.main;

        Instantiate(castGO, Camera.main.transform.position, Quaternion.identity);

        cast = castGO.GetComponent<CastSO>();
        // pathPoints = cast.pointObjects;
        
        // if(pathPoints.Count > 0) pointScore = 100 / pathPoints.Count;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

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

        if (Input.GetMouseButtonUp(0))
        {
            pathPoints = cast.pointObjects;
            if (pathPoints.Count > 0) pointScore = 100 / pathPoints.Count;

            Debug.Log(cast.pointObjects.Count + " is the count of path points list");
            foreach (GameObject point in pathPoints)
            {
                if (point.GetComponent<PathPoint>().hit == true) score += pointScore;
                Debug.Log(point.GetComponent<PathPoint>().hit);
            }
            Debug.Log("Score is " + score);

            CookingManager.instance.Transition();
            gameObject.SetActive(false);
        }
    }
}
