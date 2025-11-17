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
    Coroutine currentAnim;  //Keeps track of the current animation

    void Start()
    {
        progressBar.minValue = 0;
        progressBar.maxValue = timeSec;
        progressBar.value = 0;
        progressBar.gameObject.SetActive(true);
    }

    void Update()
    {
        //Starts the squeezing animation
        if (Input.GetMouseButtonDown(0))
        {
            if(currentAnim != null) StopCoroutine(currentAnim);
            currentAnim = StartCoroutine(ToolSqueezeAnim());
        }

        //While the player holds down the mouse, progress the timer and bar
        if (Input.GetMouseButton(0))
        {
            timer += Time.deltaTime;
            progressBar.value = timer;

            if (timer >= timeSec)
            {
                progressBar.gameObject.SetActive(false);
                CookingManager.instance.Transition();
                this.gameObject.SetActive(false);
            }
        }

        //If the player lets go of the mouse, start the release animation
        if (Input.GetMouseButtonUp(0))
        {
            if (!handSqueeze)
            {
                if (currentAnim != null) StopCoroutine(currentAnim);
                currentAnim = StartCoroutine(ToolReleaseAnim());
            }
        }
    }

    //Does the squeeze animation for the tool
    IEnumerator ToolSqueezeAnim()
    {
        while (true)
        {
            float topAngle = topOfSqueezeTool.transform.localRotation.eulerAngles.z;

            if (topAngle > 0)
            {
                topAngle -= 360;
            }
            if (topAngle > -40)
            {
                topOfSqueezeTool.transform.Rotate(0, 0, -5);
            }
            else
            {
                break;
            }
            yield return null;
        }
    }
    
    //Does the release animation for the tool
    IEnumerator ToolReleaseAnim()
    {
        while (true)
        {
            float topAngle = Mathf.RoundToInt(topOfSqueezeTool.transform.localRotation.eulerAngles.z);

            if (topAngle > 0)
            {
                topAngle -= 360;
            }
            if (topAngle < 0)
            {
                topOfSqueezeTool.transform.Rotate(0, 0, 5);
            }
            else
            {
                break;
            }

            yield return null;
        }
    }
}