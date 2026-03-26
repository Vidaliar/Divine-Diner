using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingArrow : MonoBehaviour
{
    [SerializeField] Transform parentArrow;
    Transform line;
    Transform triangle;
    // Start is called before the first frame update
    void Start()
    {
        line = parentArrow.GetChild(0);
        triangle = parentArrow.GetChild(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Need to add logic for scaling
    public void UpdateArrow(bool rightOrTop, bool vertical)
    {
        if(vertical)
        {
            if(rightOrTop)
            {
                parentArrow.rotation = Quaternion.Euler(0,0,0);
            }
            else
            {
                parentArrow.rotation = Quaternion.Euler(0,0,180);
            }
        }
        else
        {
            if(rightOrTop)
            {
                parentArrow.rotation = Quaternion.Euler(0,0,90);
            }
            else
            {
                parentArrow.rotation = Quaternion.Euler(0,0,270);
            }   
        }
    }
}
