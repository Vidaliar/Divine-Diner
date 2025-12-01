using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
================================================================================
SettingsMenuUI.cs — UI bridge for editing settings (Save / Undo / Exit)
================================================================================

IMPLEMENTATION GUIDE (How to use)

A) Scene & Inspector Setup
   1) Root GameObject:
      - Use the same GameObject that already has your SettingsMenu script.
      - Add this SettingsMenuUI component to that GameObject.
   2) Inspector References:
      - masterVolumeSlider  : Slider for master volume.
      - bgmVolumeSlider     : Slider for BGM volume.
      - fullscreenToggle    : Toggle for fullscreen flag.
      - settingsMenu        : Reference to the existing SettingsMenu component
                              (on the same GameObject).
      - rootPanel (optional): Root GameObject of the settings menu
                              (used only as a fallback if settingsMenu is null).

B) UI Event Wiring
   - Sliders & Toggle:
       masterVolumeSlider.onValueChanged  -> OnMasterVolumeChanged(float)
       bgmVolumeSlider.onValueChanged     -> OnBgmVolumeChanged(float)
       fullscreenToggle.onValueChanged    -> OnFullscreenChanged(bool)

   - Buttons:
       "Save & Exit" button       -> OnClickSaveAndExit()
       "Undo Changes" button      -> OnClickUndoChanges()
       "Exit Without Save" button -> OnClickExitWithoutSave()

C) Runtime Behavior
   - When the Settings menu root GameObject becomes active (OnEnable):
       1) Reads current settings from SettingsManager.Instance.Current.
       2) Creates two copies:
          - savedSnapshot : snapshot when menu opened (for Undo).
          - editingData   : the data being modified by the UI.
       3) Updates all UI controls from editingData.

   - While the player adjusts controls:
       - Only editingData is updated; the game's actual settings stay unchanged.

   - When "Save & Exit" is clicked:
       1) SettingsManager.Instance.OverrideCurrent(editingData);
       2) SettingsManager.Instance.SaveToDisk();
       3) SettingsManager.Instance.ApplyCurrent();
       4) CloseMenu():
          - If settingsMenu is assigned -> settingsMenu.Back(); (uses your
            existing PauseMenu flow).
          - Otherwise -> disables rootPanel / this GameObject.

   - When "Undo Changes" is clicked:
       1) editingData is reset from savedSnapshot.Clone().
       2) UI is refreshed.
       3) SettingsManager.Current is NOT modified.

   - When "Exit Without Save" is clicked:
       - CloseMenu() is called; no data is saved or applied.

D) Extending
   - When adding a new setting:
       1) Add a field in SettingsData.
       2) Add corresponding UI control + OnValueChanged handler here.
       3) Update RefreshUIFromData() to read the new field into UI.
       4) Update SettingsManager.ApplyCurrent() to apply the new field.

NOTES
- This script does not control which page (Page1/2/3/4) is visible — that is
  still handled by your existing SettingsMenu script.
- It only handles the data editing workflow and communicates with SettingsManager.
================================================================================
*/

[DisallowMultipleComponent]
public class SettingsMenuUI : MonoBehaviour
{
    [Header("UI Controls")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Menu References")]
    [SerializeField] private SettingsMenu settingsMenu;  // Existing script for pages + Back()
    [SerializeField] private GameObject rootPanel;       // Optional fallback root

    // Snapshot of settings when the menu was opened (basis for "Undo Changes")
    private SettingsData savedSnapshot;

    // Data currently being edited by UI controls
    private SettingsData editingData;

    private void OnEnable()
    {
        OpenMenu();
    }

    // Initialize editing and snapshot data when the menu opens
    public void OpenMenu()
    {
        if (SettingsManager.Instance == null)
        {
            Debug.LogError("SettingsMenuUI: SettingsManager.Instance is null. Make sure SettingsManager exists in the scene.");
            return;
        }

        SettingsData current = SettingsManager.Instance.Current;

        // Create snapshot + editing copies
        savedSnapshot = current.Clone();
        editingData = current.Clone();

        RefreshUIFromData(editingData);
    }

    // Push SettingsData values into UI controls
    private void RefreshUIFromData(SettingsData data)
    {
        if (data == null)
        {
            Debug.LogWarning("SettingsMenuUI.RefreshUIFromData called with null data.");
            return;
        }

        if (masterVolumeSlider != null)
            masterVolumeSlider.value = data.masterVolume;

        if (bgmVolumeSlider != null)
            bgmVolumeSlider.value = data.bgmVolume;

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = data.isFullScreen;
    }

    // ====================== UI VALUE CHANGE CALLBACKS =======================

    public void OnMasterVolumeChanged(float value)
    {
        if (editingData == null) return;

        editingData.masterVolume = value;

        // Optional: live preview of master volume
        // AudioListener.volume = value;
    }

    public void OnBgmVolumeChanged(float value)
    {
        if (editingData == null) return;

        editingData.bgmVolume = value;
    }

    public void OnFullscreenChanged(bool isOn)
    {
        if (editingData == null) return;

        editingData.isFullScreen = isOn;
    }

    // ============================ BUTTON ACTIONS ============================

    // Save editingData -> SettingsManager, then exit via CloseMenu()
    public void OnClickSaveAndExit()
    {
        if (editingData == null)
        {
            Debug.LogWarning("SettingsMenuUI.OnClickSaveAndExit called but editingData is null.");
            return;
        }

        if (SettingsManager.Instance == null)
        {
            Debug.LogError("SettingsMenuUI.OnClickSaveAndExit: SettingsManager.Instance is null.");
            return;
        }

        SettingsManager.Instance.OverrideCurrent(editingData);
        SettingsManager.Instance.SaveToDisk();
        SettingsManager.Instance.ApplyCurrent();

        CloseMenu();
    }

    // Restore editingData from savedSnapshot and refresh UI
    public void OnClickUndoChanges()
    {
        if (savedSnapshot == null)
        {
            Debug.LogWarning("SettingsMenuUI.OnClickUndoChanges called but savedSnapshot is null.");
            return;
        }

        editingData = savedSnapshot.Clone();
        RefreshUIFromData(editingData);
    }

    // Exit without saving anything
    public void OnClickExitWithoutSave()
    {
        CloseMenu();
    }

    // Close settings menu, preferably through existing SettingsMenu.Back()
    private void CloseMenu()
    {
        // Preferred: use existing menu logic (includes PauseMenu integration)
        if (settingsMenu != null)
        {
            settingsMenu.Back();
            return;
        }

        // Fallback: simply disable the root panel or this GameObject
        if (rootPanel != null)
        {
            rootPanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
