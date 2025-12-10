using UnityEngine;

public class QuitOnKey : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            Debug.Log("Quit requested"); // works only in editor/log, not in build
        }
    }
}
