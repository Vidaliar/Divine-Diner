using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("Main Title Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitGameButton;

    [Header("Optional Back Buttons")]
    [SerializeField] private Button backFromLoadMenuButton;
    [SerializeField] private Button backFromSettingsButton;

    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject saveLoadMenuPanel;
    [SerializeField] private GameObject settingsMenuPanel;

    [Header("New Game Scene")]
    [SerializeField] private string newGameScene = "filler";

    private void Start()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(NewGame);

        if (continueGameButton != null)
            continueGameButton.onClick.AddListener(OpenLoadMenu);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettingsMenu);

        if (exitGameButton != null)
            exitGameButton.onClick.AddListener(ExitGame);

        if (backFromLoadMenuButton != null)
            backFromLoadMenuButton.onClick.AddListener(OpenMainMenu);

        if (backFromSettingsButton != null)
            backFromSettingsButton.onClick.AddListener(OpenMainMenu);

        OpenMainMenu();
    }

    public void OpenMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (saveLoadMenuPanel != null) saveLoadMenuPanel.SetActive(false);
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(false);
    }

    public void OpenLoadMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (saveLoadMenuPanel != null) saveLoadMenuPanel.SetActive(true);
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(false);
    }

    public void OpenSettingsMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (saveLoadMenuPanel != null) saveLoadMenuPanel.SetActive(false);
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(true);
    }

    public void NewGame()
    {
        SceneManager.LoadScene(newGameScene, LoadSceneMode.Single);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}