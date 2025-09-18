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
    void OnTriggerEnter2D(Collider2D coll)
    {
        Debug.Log(this.gameObject.name + " got hit");
        hit = true;
    }
}