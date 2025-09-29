using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastManager : MonoBehaviour
{
    public static CastManager instance;
    [SerializeField] GameObject castGO;
    [SerializeField] float linePointDist = 0.3f;
    [SerializeField] GameObject drawLineCollider;
    LineRenderer line;
    Vector2 lastLinePos = new Vector2(-100, -100);
    Camera cam;
    float score = 0;
    CastSO cast;
    List<GameObject> pathPoints;
    float pointScore = 100f;
    int numOutBounds = 0;
    public bool pointsInBounds = true;

    void Start()
    {
        if (instance == null && instance != this)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        line = this.GetComponent<LineRenderer>();
        line.positionCount = 0;

        cam = Camera.main;

        GameObject newCast = Instantiate(castGO, Camera.main.transform.position, Quaternion.identity);

        cast = newCast.GetComponent<CastSO>();
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

        drawLineCollider.transform.position = pos;

        //Draws the line while the mouse button is held down
        if (Input.GetMouseButton(0))
        {
            if (Vector2.Distance(pos, lastLinePos) > linePointDist)
            {
                line.positionCount++;
                int pointIndex = line.positionCount - 1;
                line.SetPosition(pointIndex, pos);
                lastLinePos = pos;

                CheckPointBounds(pos);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            pathPoints = cast.pointObjects;
            if (pathPoints.Count > 0) pointScore = 100f / pathPoints.Count;

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

    void CheckPointBounds(Vector3 pos)
    {
        //Use OnCollisionExit2D on cast 
        //numOutBounds could be used either way to do a percentage or 'static' score system
        if (pointsInBounds == false)
        {
            Debug.Log("Score deducted by 5");
            score -= 5; //TEMP, MAYBE MAKE IT DEPEND ON numOutBounds
            numOutBounds++;
        }
    }

    public void SetBoundsBool(bool inBounds)
    {
        pointsInBounds = inBounds;
    }
}
