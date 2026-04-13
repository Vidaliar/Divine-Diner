using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [CreateAssetMenu(fileName = "Cast", menuName = "Cast")]
public class CastSO : MonoBehaviour
{
    [SerializeField] GameObject pathPointPref;
    public List<Vector2> points = new List<Vector2>();
    public List<GameObject> pointObjects; //{ get; private set; }
    public int numLines = 1;
    [SerializeField] bool useLinePoints = true;
    LineRenderer line;
    // Vector2 center;
    EdgeCollider2D edgeColl;
    void Awake()
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
                points[i] = points[i] * (worldCamHeight / 2);
                points[i] += center;
            }
            DrawCast();
        }
        else
        {
            for (int i = 0; i<line.positionCount; i++)
            {
                Vector3 point = line.GetPosition(i) * (worldCamHeight/2) + new Vector3 (center.x, center.y, 0);
                point.z = 0;
                line.SetPosition(i, point);
            }
        }

        // DrawCast();
        PlacePointMarkers();
        UpdateEdgeCollider();
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
            Debug.Log(edgeColl.points[i]);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name + " exited cast");
        if(collision.gameObject.CompareTag("Player")) CastManager.instance.SetBoundsBool(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name + " entered cast");
        if(collision.gameObject.CompareTag("Player")) CastManager.instance.SetBoundsBool(true);
    }
}