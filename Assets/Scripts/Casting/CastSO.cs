using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastSO : MonoBehaviour
{
    [SerializeField] string attribute;
    [SerializeField] GameObject pathPointPref;
    [SerializeField] bool useLinePoints = true;
    public List<Vector2> points = new List<Vector2>();
    public List<GameObject> pointObjects; //{ get; private set; }
    public int numLines = 1;
    public bool canDraw = false;
    LineRenderer line;
    // Vector2 center;
    EdgeCollider2D edgeColl;
    void Start()
    {
        line = this.GetComponent<LineRenderer>();
        Vector2 center = transform.position;
        edgeColl = this.GetComponent<EdgeCollider2D>();

        pointObjects = new List<GameObject>();
        // pointObjects = new GameObject[points.Count];

        float worldCamHeight = Camera.main.orthographicSize * 2;
        // float worldCamLength = worldCamHeight * Screen.width / Screen.height;

        if(!useLinePoints){
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = points[i] * (worldCamHeight / 2) * transform.localScale;
                Debug.Log(transform.localScale);
                points[i] += center;
            }
            DrawCast();
        }
        else
        {
            Debug.Log("Center " + center);
            for (int i = 0; i<line.positionCount; i++)
            {
                Vector3 point = line.GetPosition(i) * (worldCamHeight/2);
                Debug.Log(point);
                point = new Vector3(point.x * transform.localScale.x, point.y * transform.localScale.y, 0)+ new Vector3 (center.x, center.y, 0);
                Debug.Log("After scale "+point);
                // point.z = 0;
                line.SetPosition(i, point);
                points.Add(point);
            }
        }

        // DrawCast();
        if(canDraw){
        PlacePointMarkers();
        UpdateEdgeCollider();
        }
    }

    //For efficienty, consider combining DrawCast and UpdateEdgeCollider since they loop the same
    void DrawCast()
    {
        line.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            line.SetPosition(i, points[i]);
        }
    }

    void PlacePointMarkers()
    {
        foreach (var point in points)
        {
            GameObject newPathPoint = Instantiate(pathPointPref, point, Quaternion.identity);
            newPathPoint.GetComponent<PathPoint>().pos = newPathPoint.transform.position;
            // Debug.Log("Path point added to pointObjects " + newPathPoint.name + " " + pointNumTEMP);
            pointObjects.Add(newPathPoint);
        }
    }

    void UpdateEdgeCollider()
    {
        // edgeColl.Count = points.Count;
        edgeColl.SetPoints(points);
        for(int i=0; i<points.Count; i++)
        {
            edgeColl.points[i] = points[i];
            // Debug.Log(edgeColl.points[i]);
        }
    }

    public string GetAttribute()
    {
        return attribute;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // Debug.Log(collision.gameObject.name + " exited cast");
        if(collision.gameObject.CompareTag("Player")) CastManager.instance.SetBoundsBool(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Debug.Log(collision.gameObject.name + " entered cast");
        if(collision.gameObject.CompareTag("Player")) CastManager.instance.SetBoundsBool(true);
    }
}