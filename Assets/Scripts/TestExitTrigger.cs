using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestExitTrigger : MonoBehaviour
{
    void OnTriggerExit2D(Collider2D coll)
    {
        Debug.Log(coll.gameObject.name + " is the collider that exited according to the other obj");
    }
}
