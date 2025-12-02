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
}
