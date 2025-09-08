using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    }
}
