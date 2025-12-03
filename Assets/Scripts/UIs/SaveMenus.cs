using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;



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

    private Texture2D loadedTexture;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetEmptyVisual()
    {
        if (infoText != null)
            infoText.text = emptyText;

        if (thumbnailImage != null)
            thumbnailImage.sprite = emptySprite;

        // For empty slots: cannot load, but can decide whether delete is allowed.
        if (loadButton != null) loadButton.interactable = false;
        if (deleteButton != null) deleteButton.interactable = false;
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

    public void RefreshUI(){ }
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

        // Call into your SaveSystem to load this slot
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
