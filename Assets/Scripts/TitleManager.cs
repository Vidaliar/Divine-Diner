using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] public Button newGameButton, continueGameButton, exitGameButton, settingsButton;

    void Start()
    {
        newGameButton.onClick.AddListener(NewGame);
        continueGameButton.onClick.AddListener(ContinueGame);
        exitGameButton.onClick.AddListener(ExitGame);
        settingsButton.onClick.AddListener(settings);
    }

    
    void Update()
    {
        
    }

    void NewGame()
    {
        // Load Scene with the name, load destroys current scene
        SceneManager.LoadScene("Edison - InkySampleTestScene", LoadSceneMode.Single);
    }
    
    void ContinueGame()
    {

    }

    void ExitGame()
    {
        Application.Quit();
    }

    void settings()
    {

        // Load Settings Scene with name (assuming it's a scene). If otherwise let me know ASAP
        // additive so it doesn't destroy current scene
        SceneManager.LoadScene("TestSettings", LoadSceneMode.Additive);
    }
}
