using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Usage Guide:
/// 1. Create a dedicated CreditsScene.
/// 2. Create a Canvas and put all credits UI under one RectTransform named CreditsContent.
/// 3. Attach this script to CreditsContent.
/// 4. Set startY below the screen and endY above the screen.
/// 5. Set returnSceneName to your title scene, for example "TitleScene".
/// 6. Add CreditsScene and TitleScene to Build Settings.
/// </summary>
public class CreditsScroller : MonoBehaviour
{
    [Header("Scroll Target")]
    [SerializeField] private RectTransform creditsContent;

    [Header("Scroll Settings")]
    [SerializeField] private float startY = -700f;
    [SerializeField] private float endY = 1600f;
    [SerializeField] private float scrollSpeed = 60f;

    [Header("Scene Settings")]
    [SerializeField] private string returnSceneName = "TitleScene";
    [SerializeField] private bool returnToTitleWhenFinished = true;

    [Header("Input")]
    [SerializeField] private KeyCode skipKey = KeyCode.Escape;
    [SerializeField] private KeyCode speedUpKey = KeyCode.Space;
    [SerializeField] private float speedUpMultiplier = 3f;

    private bool finished;

    private void Awake()
    {
        if (creditsContent == null)
        {
            creditsContent = GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;

        Vector2 position = creditsContent.anchoredPosition;
        position.y = startY;
        creditsContent.anchoredPosition = position;
    }

    private void Update()
    {
        if (finished) return;

        if (Input.GetKeyDown(skipKey))
        {
            ReturnToTitle();
            return;
        }

        float currentSpeed = scrollSpeed;

        if (Input.GetKey(speedUpKey))
        {
            currentSpeed *= speedUpMultiplier;
        }

        Vector2 position = creditsContent.anchoredPosition;
        position.y += currentSpeed * Time.unscaledDeltaTime;
        creditsContent.anchoredPosition = position;

        if (position.y >= endY)
        {
            FinishCredits();
        }
    }

    private void FinishCredits()
    {
        finished = true;

        if (returnToTitleWhenFinished)
        {
            ReturnToTitle();
        }
    }

    public void ReturnToTitle()
    {
        SceneManager.LoadScene(returnSceneName);
    }
}