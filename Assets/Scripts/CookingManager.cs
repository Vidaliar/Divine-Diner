using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//use scriptable obj of recipe to contain prep ingredients and difficulty, pass those into cookingManager?
public enum CookStep
{
    Prep,
    Cast,
    Complete
}

public class CookingManager : MonoBehaviour
{
    public static CookingManager instance;
    CookStep step = CookStep.Prep;
    int numTotalPrep = 3;
    int numPrep = 1;

    void Start()
    {
        if (instance == null && instance != this)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Transition()
    {
        switch (step)
        {
            case CookStep.Prep:
                if (numPrep == numTotalPrep)
                {
                    step = CookStep.Cast;
                }
                else
                {
                    float worldCamHeight = Camera.main.orthographicSize * 2;
                    float worldCamLength = worldCamHeight * Screen.width / Screen.height;
                    Camera.main.transform.position = new Vector3(worldCamLength * numPrep, 0, -10);
                    numPrep++;
                }
                break;

            default:
                Debug.Log("Hit default in transition");
                break;
        }
    }
}
