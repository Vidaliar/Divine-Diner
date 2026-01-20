using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] public Button newGameButton, continueGameButton, exitGameButton, settingsButton;

    [Header("New Game Scene")]
    public string NewGameScene = "filler";

    bool isInPause;

    void Start()
    {
        newGameButton.onClick.AddListener(NewGame);
        continueGameButton.onClick.AddListener(ContinueGame);
        exitGameButton.onClick.AddListener(ExitGame);
        //settingsButton.onClick.AddListener(settings);
        // this is exchanged for junhao's settings
    }

    
    void Update()
    {
        
    }

    void NewGame()
    {
        // Load Scene with the name, load destroys current scene
        SceneManager.LoadScene(NewGameScene, LoadSceneMode.Single);
    }
    
    void ContinueGame()
    {
        // read most recent save file and load it
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
