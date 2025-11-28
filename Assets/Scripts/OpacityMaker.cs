using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


public class OpacityMaker : MonoBehaviour
{
    [SerializeField] float alpha;

    private Image image;
    
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        var tempColor = image.color;
        tempColor.a = alpha;
        image.color = tempColor;
    }
}
