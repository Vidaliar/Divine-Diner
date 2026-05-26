using UnityEngine;

public class ForceCustomCursorVisible : MonoBehaviour
{
    [Header("Custom Cursor")]
    [SerializeField] private Texture2D cursorTexture;

    [Tooltip("Usually top-left is (0, 0). Center would be half width and half height.")]
    [SerializeField] private Vector2 hotSpot = Vector2.zero;

    private void Awake()
    {
        ShowCookingCursor();
    }

    private void Start()
    {
        ShowCookingCursor();
    }

    private void OnEnable()
    {
        ShowCookingCursor();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            ShowCookingCursor();
        }
    }

    private void ShowCookingCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
        }
        else
        {
            Debug.LogWarning("[Cursor] Custom cursor texture is missing.");
        }
    }

    private void HideCookingCursor()
    {
        Cursor.lockState = CursorLockMode.None;

        // Remove the cooking custom cursor texture.
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        // Hide the native cursor again so the VN UI cursor is the only one visible.
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        HideCookingCursor();
    }

    private void OnDestroy()
    {
        HideCookingCursor();
    }
}