using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
-Manager for the overall cooking minigame
-Keeps track of the current step in the minigame
    -Prep, cast, or if the minigame is complete
-Transitions between each prep step, from the last prep to casting, and from casting to completion
*/

//Thought: use scriptable obj of recipe to contain prep ingredients and difficulty?, pass those into cookingManager?

//Enum to track which step the player is at for the overall recipe
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


    [Header("Return to VN")]
    [SerializeField] string vnSceneName = "ZeusBeat1";          // <-- your VN scene name
    [SerializeField] string returnSuccessNode = "Hephaestus_Return"; // <-- node to resume
    [SerializeField] string returnFailNode = "Hephaestus_Return";
    [SerializeField] float returnDelay = 0.5f;


    [Header("Managers")]
    [SerializeField] List<GameObject> recipeManagers;   //Holds all of the required preparation managers
    [SerializeField] GameObject castManager;
    [SerializeField] CameraMover cameraManager;

    [Header("Objects")]
    [SerializeField] GameObject finalFood;
    int numTotalPrep;
    int numPrep = 1;    //Keeps track of the current preparation step
    float worldCamHeight;
    float worldCamLength;   //Uses this to make the separation of steps be related to the user's screen size (I think)
    bool canActivateCast;
    public bool cookingSuccess;

    public bool inPause = false;

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

        numTotalPrep = recipeManagers.Count;

        // AudioManager.Instance.PlaySound("KitchenBackground");    //Use once stopping audio is solved
    }

    //Waits for canActivateCast to be true to 'turn on' the cast
    void Update()
    {
        if(canActivateCast && step == CookStep.Cast)
        {
            // if (Input.GetMouseButtonUp(0))
            // {
                castManager.SetActive(true);
                canActivateCast = false;
            // }
        }
        else if(step == CookStep.Cast)
        {
            if (Input.GetMouseButtonUp(0))
            {
                castManager.SetActive(true);
                canActivateCast = false;
            }
        }
    }

    //Called by recipe managers to go to the next section
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
                    //castManager.SetActive(true);
                    if(Input.GetMouseButton(0))
                    {
                        canActivateCast = false;
                    }
                }
                else
                {
                    Camera.main.transform.position = new Vector3(worldCamLength * numPrep, 0, -10);
                    recipeManagers[numPrep].transform.parent.position = new Vector3(worldCamLength * numPrep, 0, 0);
                    recipeManagers[numPrep].SetActive(true);
                    numPrep++;
                }
                break;

            case CookStep.Cast:
                Camera.main.transform.position = new Vector3(worldCamLength * (numPrep++), 0, -10);
                finalFood.SetActive(true);
                finalFood.transform.position = new Vector2(Camera.main.transform.position.x, finalFood.transform.position.y);
                step = CookStep.Complete;

                // Auto-return after a short beat:
                if (returnDelay > 0f) Invoke(nameof(FinishCooking), returnDelay);
                else FinishCooking();
                break;


            default:
                Debug.Log("Hit default in transition");
                break;
        }
    }

    //Transitions back to the visual novel
    void FinishCooking()
    {
        VNReturn.NextNode = cookingSuccess ? returnSuccessNode : returnFailNode;
        SceneManager.LoadScene(vnSceneName);
    }
}
