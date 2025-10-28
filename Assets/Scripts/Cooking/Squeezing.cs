using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Squeezing : MonoBehaviour
{
    // GameObject squeezeIngredient;    Likely to be used to differentiate the four types
    [SerializeField] GameObject topOfSqueezeTool;
    [SerializeField] Slider progressBar;
    float timeSec = 5.5f;   //Total time needs to be squeezed
    bool handSqueeze = false;   //If it is hand squeezing or tool squeezing
    float timer;    //Keeps track of time passed

    void Start()
    {
        progressBar.minValue = 0;
        progressBar.maxValue = timeSec;
        progressBar.value = 0;
        progressBar.gameObject.SetActive(true);
    }

    void Update()
    {
        // if (Input.GetMouseButtonDown(0))
        // {
        //     StartCoroutine(ToolSqueezeAnim());
        // }
        
        if(Input.GetMouseButton(0))
        {
            if (!handSqueeze)
            {

            }
            //Do 'animation'
            timer += Time.deltaTime;
            progressBar.value = timer;

            if (timer >= timeSec)
            {
                progressBar.gameObject.SetActive(false);
                CookingManager.instance.Transition();
                this.gameObject.SetActive(false);
            }
        }
    }
    
    IEnumerator ToolSqueezeAnim()
    {
        // float topAngle = topOfSqueezeTool.transform.localRotation.eulerAngles.z;
        for (int i = 0; i<8; i++)
        {
            topOfSqueezeTool.transform.Rotate(0, 0, -5);
            // if(topAngle <= -40 || topAngle <= 320)
            // {
            //     yield break;
            // }
            // topAngle = topOfSqueezeTool.transform.localRotation.eulerAngles.z;
            yield return null;
        }
        // while(topAngle > -40 || topAngle > 320)
        // {
        //     Debug.Log(topAngle);
        //     topOfSqueezeTool.transform.Rotate(0, 0, -5);
        //     if(topAngle <= -40 || topAngle <= 320)
        //     {
        //         yield break;
        //     }
        //     topAngle = topOfSqueezeTool.transform.localRotation.eulerAngles.z;
        //     yield return null;
        // }
    }
}