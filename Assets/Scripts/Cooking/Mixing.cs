using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Mixing : MonoBehaviour
{
    [SerializeField] GameObject controlsText;
    [SerializeField] Transform shaker;
    [SerializeField] int totalMixes = 10;

    Vector2 prevPos;

    int mixCount = 0;

    void Start()
    {
        controlsText.SetActive(true);

    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {

        }

        if (mixCount >= totalMixes)
        {
            CookingManager.instance.Transition();
            controlsText.SetActive(false);
            gameObject.SetActive(false);
        }
    }
    
    /*
    while mousebutton, detect if mouse is going in a circle


    */
}
