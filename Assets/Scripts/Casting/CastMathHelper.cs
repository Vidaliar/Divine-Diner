using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastMathHelper : MonoBehaviour
{
    public GameObject cast;
    public float scale = 0.10f;
    // Start is called before the first frame update
    void Start()
    {
        LineRenderer line = cast.GetComponent<LineRenderer>();

        // foreach(Vector3 point in line.points)
        // {
        //     point.x = point.x/scale;
        //     point.y = point.y/scale;
        //     point.z = 0;
        // }

        for (int i = 0; i<line.positionCount; i++)
            {
                Vector3 point = line.GetPosition(i) * scale;
                // point = new Vector3(point.x * transform.localScale.x, point.y * transform.localScale.y, 0);
                point.z = 0;
                line.SetPosition(i, point);
            }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
