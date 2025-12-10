using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;



/// <summary>
/// SaveMenus
/// ----------------------------------------------
/// HOW TO USE (per save slot page / per slot item)
/// ----------------------------------------------
/// 1. In your scene, make sure you already have:
///    - A SaveSystem component in some GameObject.
///    - SaveSystem is responsible for writing JSON & PNG when saving.
/// 
/// 2. For EACH save slot menu page (or each slot item in a list):
///    - Create a root GameObject for this slot page.
///    - Under this root, you should have:
///        * An Image component used to show the screenshot thumbnail.
///        * A TMP_Text (TextMeshProUGUI) used to show save info (title + time).
///        * A Button for "Load".
///        * A Button for "Delete".
/// 
/// 3. Attach this SaveMenus script to the root GameObject of that slot page.
/// 
/// 4. In the inspector of SaveMenus:
///    - Save System      : drag your SaveSystem component here.
///    - Profile Name     : usually "Default" (or any profile string you use).
///    - Slot Index       : 0 for the first slot, 1 for the second, 2, 3, ...
///    - Thumbnail Image  : drag the Image you want to display the screenshot.
///    - Info Text        : drag the TMP_Text that shows save info.
///    - Load Button      : drag the "Load" button.
///    - Delete Button    : drag the "Delete" button.
///    - Empty Sprite     : (optional) sprite used for empty slot.
///    - Empty Text       : text shown when there is no save in this slot.
/// 
/// 5. Multi-slot menu usage:
///    - If you have 4 different slot pages:
///        * Page for Slot 0: SaveMenus.slotIndex = 0
///        * Page for Slot 1: SaveMenus.slotIndex = 1
///        * Page for Slot 2: SaveMenus.slotIndex = 2
///        * Page for Slot 3: SaveMenus.slotIndex = 3
///      Each page will show its own screenshot & info and loads/deletes only its own slot.
/// 
///    - If you have a single menu with 4 slot items at the same time:
///        * Put 4 GameObjects as "slot item roots".
///        * Attach one SaveMenus to each item, with slotIndex 0 / 1 / 2 / 3.
///        * All of them share the same SaveSystem reference.
/// 
/// 6. When the menu (or each slot item) is enabled:
///    - OnEnable() will call RefreshUI()
///    - RefreshUI() will:
///        * Check if there is a save in this slot (profile + slotIndex)
///        * If yes: 
///            - Read SaveMeta via saveSystem.GetMeta(...)
///            - Set info text (title + subtitle)
///            - Load PNG from saveSystem.GetThumbnailPath(...) and display it.
///            - Enable Load / Delete buttons.
///        * If no:
///            - Show Empty Sprite / Empty Text.
///            - Disable Load / Delete buttons.
/// 
/// 7. When user presses:
///    - Load Button:
///        * Call saveSystem.Load(profileName, slotIndex)
///    - Delete Button:
///        * Call saveSystem.Delete(profileName, slotIndex)
///        * Then RefreshUI() to show empty state.
/// </summary>
public class SaveMenus : MonoBehaviour
{
    [Header("Save slot config")]
    [SerializeField] private string profileName = "Default";
    [SerializeField] private int slotIndex = 0;
    [SerializeField] private SaveSystem saveSystem;

    [Header("UI references")]
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;

    [Header("Empty slot visuals")]
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private string emptyText = "Empty Slot";

    // runtime texture created from PNG file
    private Texture2D loadedTexture;

    private void OnEnable()
    {
        RegisterButtonEvents();
        RefreshUI();
    }

    private void OnDisable()
    {
        UnregisterButtonEvents();
    }

    /// <summary>
    /// Manually call this if you save to this slot while the menu is open
    /// and you want to update the thumbnail & info immediately.
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

        // ----- Read meta (title, subtitle, thumbnail path) -----
        SaveMeta meta = saveSystem.GetMeta(profileName, slotIndex);

        if (infoText != null)
        {
            if (!string.IsNullOrEmpty(meta.title) || !string.IsNullOrEmpty(meta.subtitle))
            {
                // Example:
                // Day 1 Ep 2
                // 2025-12-02 18:30
                infoText.text = $"{meta.title}\n{meta.subtitle}";
            }
            else
            {
                infoText.text = $"Slot {slotIndex}";
            }
        }

        // ----- Read screenshot PNG and show it -----
        if (thumbnailImage != null)
        {
            string path = saveSystem.GetThumbnailPath(profileName, slotIndex);

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(path);

                    // destroy previous texture if any
                    if (loadedTexture != null)
                    {
                        Destroy(loadedTexture);
                        loadedTexture = null;
                    }

                    loadedTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (loadedTexture.LoadImage(bytes))
                    {
                        var sprite = Sprite.Create(
                            loadedTexture,
                            new Rect(0, 0, loadedTexture.width, loadedTexture.height),
                            new Vector2(0.5f, 0.5f)
                        );
                        thumbnailImage.sprite = sprite;
                    }
                    else
                    {
                        Debug.LogWarning($"[SaveMenus] Failed to load image from bytes: {path}");
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

        // When there is a save, both buttons are usable
        if (loadButton != null) loadButton.interactable = true;
        if (deleteButton != null) deleteButton.interactable = true;
    }

    /// <summary>
    /// Set visuals for an empty slot (no save file).
    /// </summary>
    private void SetEmptyVisual()
    {
        if (infoText != null)
            infoText.text = emptyText;

        if (thumbnailImage != null)
            thumbnailImage.sprite = emptySprite;

        // For empty slots: cannot load or delete.
        if (loadButton != null) loadButton.interactable = false;
        if (deleteButton != null) deleteButton.interactable = false;
    }

    /// <summary>
    /// Optional helper for dynamic switching between slots using one SaveMenus.
    /// Not required if you simply use one SaveMenus per slot with fixed slotIndex.
    /// </summary>
    public void SetSlot(int newSlotIndex, string newProfileName = null)
    {
        if (!string.IsNullOrEmpty(newProfileName))
        {
            profileName = newProfileName;
        }

        slotIndex = newSlotIndex;
        RefreshUI();
    }

    private void RegisterButtonEvents()
    {
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
        if (loadButton != null)
            loadButton.onClick.RemoveListener(OnLoadClicked);

        if (deleteButton != null)
            deleteButton.onClick.RemoveListener(OnDeleteClicked);
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

        // Call into SaveSystem to load this slot
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

        // Delete save file & thumbnail via SaveSystem
        saveSystem.Delete(profileName, slotIndex);

        // Refresh UI to show empty slot
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (loadedTexture != null)
        {
            Destroy(loadedTexture);
            loadedTexture = null;
        }
    }
}