using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPoint : MonoBehaviour
{
    public Vector2 pos;
    public bool hit; //{ get; private set; }
    void Awake()
    {
        hit = false;
    }

    //First PathPoint isn't being triggered
    void OnTriggerStay2D(Collider2D coll)
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
        {
            hit = true;
        }
    }
}