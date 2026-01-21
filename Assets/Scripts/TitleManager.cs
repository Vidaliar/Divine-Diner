using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class TitleManager : MonoBehaviour
{
    [SerializeField] public Button newGameButton, continueGameButton, exitGameButton, settingsButton;
    [SerializeField] public SaveSystem saveSystem;

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
        SceneManager.LoadScene("ZeusBeat1", LoadSceneMode.Single);
    }
    
    void ContinueGame()
    {
        // read most recent save file and load it
        const string autoProfile = "AutoSave";
        const int autoSlotIndex = 0;

        if (saveSystem == null)
        {
            Debug.LogWarning("[TitleMenu] SaveSystem reference is missing. Cannot continue game.");
            return;
        }

        if (!saveSystem.HasSave(autoProfile, autoSlotIndex))
        {
            Debug.Log("[TitleMenu] No auto-save found. Starting new game instead.");
            NewGame();
            return;
        }

        var file = saveSystem.ReadSaveFile(autoProfile, autoSlotIndex);
        if (file == null || file.data == null)
        {
            Debug.LogWarning("[TitleMenu] Failed to read auto-save file. Starting new game instead.");
            NewGame();
            return;
        }

        string sceneToLoad = string.IsNullOrEmpty(file.data.sceneName)
            ? "ZeusBeat1"
            : file.data.sceneName;

        GlobalLoadContext.Request(autoProfile, autoSlotIndex);

        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);

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
