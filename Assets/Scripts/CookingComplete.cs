using UnityEngine;
using UnityEngine.SceneManagement;

public class CookingComplete : MonoBehaviour
{
    [Header("Return settings")]
    public string vnSceneName = "ZeusBeat1";          // 
    public string returnNode = "Hermes_test_Success"; // 

    public void FinishCooking()
    {
        VNReturn.NextNode = returnNode;
        SceneManager.LoadScene(vnSceneName);
    }
}

