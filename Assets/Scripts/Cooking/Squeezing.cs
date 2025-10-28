using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Squeezing : MonoBehaviour
{
    // GameObject squeezeIngredient;    Likely to be used to differentiate the four types
    [SerializeField] Slider progressBar;
    float timeSec = 5.5f;   //Total time needs to be squeezed
    bool handSqueeze = false;   //If it is hand squeezing or tool squeezing
    float timer;    //Keeps track of time passed

    void Start()
    {
        progressBar.minValue = 0;
        progressBar.maxValue = timeSec;
        progressBar.gameObject.SetActive(true);
    }
    
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
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
}