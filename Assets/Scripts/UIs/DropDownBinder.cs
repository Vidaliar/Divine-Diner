using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// DropDownBinder
/// --------------------------------------------------
/// Purpose:
///   Binds a TMP_Dropdown to multiple SaveMenus panels.
///   Only one panel is visible at a time, depending on the
///   currently selected option in the dropdown.
///
/// Intended setup:
///   - The dropdown options are:
///       0 -> "Auto Save"
///       1 -> "Save 1"
///       2 -> "Save 2"
///       3 -> "Save 3"
///       4 -> "Save 4"
///       5 -> "Save 5"
///
///   - You have one panel GameObject for each option:
///       * Auto-save panel    (with a SaveMenus component)
///       * Save 1 panel       (SaveMenus)
///       * Save 2 panel       (SaveMenus)
///       * ...
///       * Save 5 panel       (SaveMenus)
///
///   - Each panel has its own SaveMenus configured:
///       * Auto-save panel:   profileName = "AutoSave", slotIndex = 0
///       * Save 1 panel:      profileName = "Default", slotIndex = 0
///       * Save 2 panel:      profileName = "Default", slotIndex = 1
///       * Save 3 panel:      profileName = "Default", slotIndex = 2
///       * Save 4 panel:      profileName = "Default", slotIndex = 3
///       * Save 5 panel:      profileName = "Default", slotIndex = 4
///
///   - This binder:
///       * Shows only the panel matching dropdown index.
///       * Hides all other panels.
///       * Calls RefreshUI() on the newly shown panel.
/// --------------------------------------------------
/// </summary>
public class DropDownBinder : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown dropdown;

    [Header("Panels")]
    public SaveMenus autoSavePanel;   // panel for "Auto Save" (index 0)
    public SaveMenus[] manualPanels;  // panels for "Save 1".."Save 5" (indices 1..5)

    private void Awake()
    {
        if (dropdown == null)
            dropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnEnable()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.AddListener(OnDropdownChanged);
            OnDropdownChanged(dropdown.value);
        }
    }

    private void OnDisable()
    {
        if (dropdown != null)
            dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    private void OnDropdownChanged(int index)
    {
        ShowPanelForIndex(index);
    }

    private void ShowPanelForIndex(int index)
    {
        // Hide all panels first
        if (autoSavePanel != null)
            autoSavePanel.gameObject.SetActive(false);

        if (manualPanels != null)
        {
            for (int i = 0; i < manualPanels.Length; i++)
            {
                if (manualPanels[i] != null)
                    manualPanels[i].gameObject.SetActive(false);
            }
        }

        // Index 0 -> auto-save panel
        if (index == 0)
        {
            if (autoSavePanel != null)
            {
                autoSavePanel.gameObject.SetActive(true);
                autoSavePanel.RefreshUI();
            }
            return;
        }

        // Index 1..N -> manual save panels
        int manualIndex = index - 1; // Save 1 -> manualPanels[0], Save 2 -> manualPanels[1], etc.

        if (manualPanels == null)
            return;

        if (manualIndex < 0 || manualIndex >= manualPanels.Length)
            return;

        SaveMenus panel = manualPanels[manualIndex];
        if (panel != null)
        {
            panel.gameObject.SetActive(true);
            panel.RefreshUI();
        }
    }
}
