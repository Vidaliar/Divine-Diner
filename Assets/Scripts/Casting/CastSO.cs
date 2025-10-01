using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [CreateAssetMenu(fileName = "Cast", menuName = "Cast")]
public class CastSO : MonoBehaviour
{
    [SerializeField] GameObject pathPointPref;
    public List<Vector2> points = new List<Vector2>();
    public List<GameObject> pointObjects; //{ get; private set; }
    LineRenderer line;
    Vector2 center;
    void Awake()
    {
        line = this.GetComponent<LineRenderer>();
        center = transform.position;

        pointObjects = new List<GameObject>();
        // pointObjects = new GameObject[points.Count];

        float worldCamHeight = Camera.main.orthographicSize * 2;
        // float worldCamLength = worldCamHeight * Screen.width / Screen.height;

        for (int i = 0; i < points.Count; i++)
        {
            points[i] = points[i] * (worldCamHeight / 2);
            points[i] += center;
        }

        DrawCast();
        PlacePointMarkers();
    }

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