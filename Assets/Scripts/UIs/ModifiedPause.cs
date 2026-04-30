using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModifiedPause : MonoBehaviour
{
    [Header("Settings in Main Menu")]
    public Button settingsInMainMenuButton;
    
    [SerializeField] string MainMenuSceneName = "TitleScene";

    [Header("First class menu")]
    public GameObject background;
    public GameObject BGBlock;
    public GameObject level1Menu;
    public Button resumeButton;
    public Button saveLoadButton;
    public Button settingsButton;
    public Button QuitButton;
    

    [Header("secondary menu")]
    public GameObject saveMenu;
    public GameObject settingsMenu;

    [Header("background")]
    [SerializeField] GameObject blurOverlay;

    [Header("mouse?")]
    public bool manageCursor = true;

    [Header("fade")]
    public float fadeDuration = 0.15f;

    private bool paused;

    void Awake()
    {
        // inactive all
        if (BGBlock) BGBlock.SetActive(false);
        if (background) background.SetActive(false);
        if (level1Menu) level1Menu.SetActive(false);
        if (saveMenu) saveMenu.SetActive(false);
        if (settingsMenu) settingsMenu.SetActive(false);
        if (blurOverlay) blurOverlay.SetActive(false);

        paused = false;
    }

    private void Start()
    {
        if (settingsInMainMenuButton != null)
            settingsInMainMenuButton.onClick.AddListener(PauseAndOpenLevel1);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnClick_Resume);

        if (saveLoadButton != null)
            saveLoadButton.onClick.AddListener(OnClick_OpenSaveMenu);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnClick_OpenSettings);

        if (QuitButton != null)
            QuitButton.onClick.AddListener(OnClick_Quit);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // if secondary menu are open: back to first
            if ((saveMenu && saveMenu.activeSelf) || (settingsMenu && settingsMenu.activeSelf))
            {
                CloseSecondLevelAndReturnLevel1();
                return;
            }

            // do resume
            if (paused) ResumeGame();
            else PauseAndOpenLevel1();
        }
    }

    // OnClicks
    public void OnClick_Resume()
    {
        ResumeGame();
    }

    public void OnClick_OpenSaveMenu()
    {
        OpenSecondLevel(saveMenu);
    }

    public void OnClick_OpenSettings()
    {
        OpenSecondLevel(settingsMenu);
    }

    public void OnClick_Quit()
    {
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private void PauseAndOpenLevel1()
    {
        paused = true;

        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (manageCursor)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (BGBlock) BGBlock.SetActive(true);
        if (background) background.SetActive(true);
        if (blurOverlay) blurOverlay.SetActive(true);
        if (level1Menu) { level1Menu.SetActive(true); level1Menu.transform.SetAsLastSibling(); }
        if (blurOverlay) blurOverlay.transform.SetAsFirstSibling();
        if (saveMenu) saveMenu.SetActive(false);
        if (settingsMenu) settingsMenu.SetActive(false);
    }

    private void ResumeGame()
    {
        paused = false;

        if (BGBlock) BGBlock.SetActive(false);
        if (background) background.SetActive(false);
        if (level1Menu) level1Menu.SetActive(false);
        if (saveMenu) saveMenu.SetActive(false);
        if (settingsMenu) settingsMenu.SetActive(false);

        ForceResumeTime();

        if (manageCursor)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (blurOverlay) blurOverlay.SetActive(false);
    }

    private void OpenSecondLevel(GameObject menu)
    {
        if (!menu) return;

        paused = true;

        if (level1Menu) level1Menu.SetActive(false);
        menu.SetActive(true);

        if (blurOverlay) blurOverlay.SetActive(true);
        if (manageCursor)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void CloseSecondLevelAndReturnLevel1()
    {
        if (saveMenu) saveMenu.SetActive(false);
        if (settingsMenu) settingsMenu.SetActive(false);

        if (level1Menu)
        {
            level1Menu.SetActive(true);
            level1Menu.transform.SetAsLastSibling();
        }

        if (blurOverlay) blurOverlay.SetActive(true);

        if (manageCursor)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private static void ForceResumeTime()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

}
