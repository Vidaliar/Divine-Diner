using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SaveMenus
/// Single shared save detail panel controller.
/// 
/// Works with:
/// - one shared detail panel on the right
/// - one selector on the left (Dropdown or TMP_Dropdown)
/// - one SaveSystem in scene
/// 
/// It controls:
/// - current selected slot
/// - screenshot preview
/// - info text
/// - Save / Load / Delete buttons
/// </summary>
public class SaveMenus : MonoBehaviour
{
    [Header("Save slot config")]
    [SerializeField] private string profileName = "Default";

    [Tooltip("The actual save slot number currently shown by this panel.")]
    [SerializeField] private int slotIndex = 1;

    [Tooltip("If using a dropdown, option 0 maps to this slot number.")]
    [SerializeField] private int firstSelectableSlotIndex = 1;

    [SerializeField] private SaveSystem saveSystem;

    [Header("Selector (optional)")]
    [SerializeField] private Dropdown slotDropdown;
    [SerializeField] private TMP_Dropdown tmpSlotDropdown;

    [Header("UI references")]
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;

    [Header("Empty slot visuals")]
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private string emptyText = "Empty Slot";

    [Header("After save")]
    [SerializeField] private float refreshDelayAfterSave = 0.15f;

    private Texture2D loadedTexture;

    private void Awake()
    {
        RegisterButtonEvents();
        RegisterDropdownEvents();
    }

    private void OnEnable()
    {
        Debug.Log(
            $"[SaveMenus] OnEnable | name={gameObject.name} | scene={gameObject.scene.name} | instanceID={GetInstanceID()} | saveSystem={(saveSystem != null ? saveSystem.name : "NULL")}"
        );

        if (saveSystem == null)
        {
            saveSystem = FindFirstObjectByType<SaveSystem>();
            Debug.Log($"[SaveMenus] Auto-find SaveSystem => {(saveSystem != null ? saveSystem.name : "NULL")}");
        }

        RegisterButtonEvents();
        RegisterDropdownEvents();

        RefreshUI();
    }

    private void OnDisable()
    {
        UnregisterButtonEvents();
        UnregisterDropdownEvents();
    }

    private void OnDestroy()
    {
        if (loadedTexture != null)
        {
            Destroy(loadedTexture);
            loadedTexture = null;
        }
    }

    /// <summary>
    /// Refresh current panel using current profileName + slotIndex.
    /// </summary>
    public void RefreshUI()
    {
        if (saveSystem == null)
        {
            Debug.LogWarning("[SaveMenus] SaveSystem reference is missing.");
            SetEmptyVisual();
            return;
        }

        bool hasSave = saveSystem.HasSave(profileName, slotIndex);

        if (!hasSave)
        {
            SetEmptyVisual();
            return;
        }

        SaveMeta meta = saveSystem.GetMeta(profileName, slotIndex);

        if (infoText != null)
        {
            if (!string.IsNullOrEmpty(meta.title) || !string.IsNullOrEmpty(meta.subtitle))
            {
                infoText.text = $"{meta.title}\n{meta.subtitle}";
            }
            else
            {
                infoText.text = $"Slot {slotIndex}";
            }
        }

        if (thumbnailImage != null)
        {
            string path = saveSystem.GetThumbnailPath(profileName, slotIndex);

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(path);

                    if (loadedTexture != null)
                    {
                        Destroy(loadedTexture);
                        loadedTexture = null;
                    }

                    loadedTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

                    if (loadedTexture.LoadImage(bytes))
                    {
                        Sprite sprite = Sprite.Create(
                            loadedTexture,
                            new Rect(0, 0, loadedTexture.width, loadedTexture.height),
                            new Vector2(0.5f, 0.5f)
                        );

                        thumbnailImage.sprite = sprite;
                    }
                    else
                    {
                        Debug.LogWarning($"[SaveMenus] Failed to load image bytes: {path}");
                        thumbnailImage.sprite = emptySprite;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SaveMenus] Exception when loading thumbnail: {path}\n{e}");
                    thumbnailImage.sprite = emptySprite;
                }
            }
            else
            {
                thumbnailImage.sprite = emptySprite;
            }
        }

        // Existing save: all buttons usable
        if (saveButton != null) saveButton.interactable = true;
        if (loadButton != null) loadButton.interactable = true;
        if (deleteButton != null) deleteButton.interactable = true;
    }

    private void SetEmptyVisual()
    {
        if (infoText != null)
            infoText.text = emptyText;

        if (thumbnailImage != null)
            thumbnailImage.sprite = emptySprite;

        // Empty slot: Save is still allowed, Load/Delete are not
        if (saveButton != null) saveButton.interactable = true;
        if (loadButton != null) loadButton.interactable = false;
        if (deleteButton != null) deleteButton.interactable = false;
    }

    /// <summary>
    /// Directly switch to a concrete slot number.
    /// Example: SetSlot(3) means "now show slot3".
    /// </summary>
    public void SetSlot(int newSlotIndex, string newProfileName = null)
    {
        if (!string.IsNullOrEmpty(newProfileName))
            profileName = newProfileName;

        slotIndex = Mathf.Max(0, newSlotIndex);
        RefreshUI();
    }

    /// <summary>
    /// Button/Inspector-friendly wrapper.
    /// </summary>
    public void SetSlotByIndex(int newSlotIndex)
    {
        SetSlot(newSlotIndex);
    }

    /// <summary>
    /// Dropdown option index -> actual save slot number.
    /// Example if firstSelectableSlotIndex = 1:
    /// option 0 => slot 1
    /// option 1 => slot 2
    /// option 2 => slot 3
    /// </summary>
    public void SetSlotBySelectionIndex(int selectionIndex)
    {
        int actualSlot = firstSelectableSlotIndex + selectionIndex;
        SetSlot(actualSlot);
    }

    public void SetSlot1() => SetSlot(1);
    public void SetSlot2() => SetSlot(2);
    public void SetSlot3() => SetSlot(3);
    public void SetSlot4() => SetSlot(4);
    public void SetSlot5() => SetSlot(5);

    private void RegisterButtonEvents()
    {
        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(OnSaveClicked);
            saveButton.onClick.AddListener(OnSaveClicked);
        }

        if (loadButton != null)
        {
            loadButton.onClick.RemoveListener(OnLoadClicked);
            loadButton.onClick.AddListener(OnLoadClicked);
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveListener(OnDeleteClicked);
            deleteButton.onClick.AddListener(OnDeleteClicked);
        }
    }

    private void UnregisterButtonEvents()
    {
        if (saveButton != null)
            saveButton.onClick.RemoveListener(OnSaveClicked);

        if (loadButton != null)
            loadButton.onClick.RemoveListener(OnLoadClicked);

        if (deleteButton != null)
            deleteButton.onClick.RemoveListener(OnDeleteClicked);
    }

    private void RegisterDropdownEvents()
    {
        if (slotDropdown != null)
        {
            slotDropdown.onValueChanged.RemoveListener(OnLegacyDropdownChanged);
            slotDropdown.onValueChanged.AddListener(OnLegacyDropdownChanged);
        }

        if (tmpSlotDropdown != null)
        {
            tmpSlotDropdown.onValueChanged.RemoveListener(OnTMPDropdownChanged);
            tmpSlotDropdown.onValueChanged.AddListener(OnTMPDropdownChanged);
        }
    }

    private void UnregisterDropdownEvents()
    {
        if (slotDropdown != null)
            slotDropdown.onValueChanged.RemoveListener(OnLegacyDropdownChanged);

        if (tmpSlotDropdown != null)
            tmpSlotDropdown.onValueChanged.RemoveListener(OnTMPDropdownChanged);
    }

    private void OnLegacyDropdownChanged(int value)
    {
        SetSlotBySelectionIndex(value);
    }

    private void OnTMPDropdownChanged(int value)
    {
        SetSlotBySelectionIndex(value);
    }

    private void OnSaveClicked()
    {
        if (saveSystem == null)
        {
            Debug.LogWarning("[SaveMenus] Cannot save, SaveSystem reference is missing.");
            return;
        }

        saveSystem.SaveCurrentToSlot(profileName, slotIndex);

        if (isActiveAndEnabled && gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshAfterSave());
        }
    }

    private void OnLoadClicked()
    {
        if (saveSystem == null)
        {
            Debug.LogWarning("[SaveMenus] Cannot load, SaveSystem reference is missing.");
            return;
        }

        if (!saveSystem.HasSave(profileName, slotIndex))
        {
            Debug.LogWarning($"[SaveMenus] No save in slot {slotIndex} to load.");
            return;
        }

        saveSystem.Load(profileName, slotIndex);
    }

    private void OnDeleteClicked()
    {
        if (saveSystem == null)
        {
            Debug.LogWarning("[SaveMenus] Cannot delete, SaveSystem reference is missing.");
            return;
        }

        if (!saveSystem.HasSave(profileName, slotIndex))
        {
            Debug.LogWarning($"[SaveMenus] No save in slot {slotIndex} to delete.");
            return;
        }

        saveSystem.Delete(profileName, slotIndex);
        RefreshUI();
    }

    private IEnumerator RefreshAfterSave()
    {
        yield return new WaitForSecondsRealtime(refreshDelayAfterSave);
        RefreshUI();
    }
}