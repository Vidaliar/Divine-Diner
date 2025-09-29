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



    [SerializeField] List<GameObject> recipeManagers;
    [SerializeField] GameObject castManager;
    [SerializeField] GameObject finalFood;
    int numTotalPrep = 3;
    int numPrep = 1;
    float worldCamHeight;
    float worldCamLength;
    bool canActivateCast;

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

        worldCamHeight = Camera.main.orthographicSize * 2;
        worldCamLength = worldCamHeight * Screen.width / Screen.height;
    }

    // Update is called once per frame
    void Update()
    {
        if (canActivateCast)
        {
            if (Input.GetMouseButtonUp(0))
            {
                castManager.SetActive(true);
                canActivateCast = false;
            }
        }
    }

    public void Transition()
    {
        switch (step)
        {
            case CookStep.Prep:
                if (numPrep == numTotalPrep)
                {
                    step = CookStep.Cast;
                    Camera.main.transform.position = new Vector3(worldCamLength * numPrep++, 0, -10);
                    canActivateCast = true;
                }
                else
                {
                    Camera.main.transform.position = new Vector3(worldCamLength * numPrep, 0, -10);
                    recipeManagers[numPrep].SetActive(true);
                    numPrep++;
                }
                break;

            case CookStep.Cast:
                Camera.main.transform.position = new Vector3(worldCamLength * (numPrep++), 0, -10);
                finalFood.SetActive(true);
                finalFood.transform.position = new Vector2(Camera.main.transform.position.x, 0);
                step = CookStep.Complete;
                break;

            default:
                Debug.Log("Hit default in transition");
                break;
        }
    }
}
