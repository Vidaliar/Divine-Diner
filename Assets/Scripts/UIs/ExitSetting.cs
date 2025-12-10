using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ExitSetting : MonoBehaviour
{
    [SerializeField] public Button exitbutton;
    void Start()
    {
        exitbutton.onClick.AddListener(exit);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void exit()
    {
        SceneManager.UnloadSceneAsync("TestSettings");
    }
}
