using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;





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


    [Header("Return to VN")]
    [SerializeField] string vnSceneName = "ZeusBeat1";          // <-- your VN scene name
    [SerializeField] string returnNode = "Hermes_test_Success"; // <-- node to resume
    [SerializeField] float returnDelay = 0.5f;


    [SerializeField] List<GameObject> recipeManagers;
    [SerializeField] GameObject castManager;
    [SerializeField] GameObject finalFood;
    int numTotalPrep;
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

        numTotalPrep = recipeManagers.Count;
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
                
                // Auto-return after a short beat:
                if (returnDelay > 0f) Invoke(nameof(FinishCooking), returnDelay);
                else FinishCooking();
                break;


            default:
                Debug.Log("Hit default in transition");
                break;
        }
    }

    void FinishCooking()
    {
        VNReturn.NextNode = "Hermes_test_Success";  // 
        SceneManager.LoadScene("ZeusBeat1");        // your VN scene name

    }
}
