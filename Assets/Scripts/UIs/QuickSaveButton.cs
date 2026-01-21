using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// QuickSaveButton
/// Writes the current game state into a fixed save slot,
/// e.g. profile "Default", slotIndex 0 (your Save 1).
/// Attach this to your Pause Menu "Quick Save" button,
/// then hook QuickSave() in the Button's OnClick.
/// </summary>
public class QuickSaveButton : MonoBehaviour
{
    [Header("References")]
    public SaveSystem saveSystem;
    public SaveMenus save1Panel;

    [Header("Target slot")]
    public string profileName = "Default";
    public int slotIndex = 0;

    public void QuickSave()
    {
        Debug.Log("[QuickSaveButton] QuickSave() called.");

        if (saveSystem == null)
        {
            Debug.LogWarning("[QuickSaveButton] SaveSystem reference is missing. Quick save aborted.");
            return;
        }

        saveSystem.SaveCurrentToSlot(profileName, slotIndex);
        Debug.Log($"[QuickSaveButton] Requested quick save to {profileName}/slot{slotIndex}");

        if (save1Panel != null)
        {
            Debug.Log("[QuickSaveButton] Refreshing Save 1 panel UI after quick save.");
            save1Panel.RefreshUI();
        }
        else
        {
            Debug.Log("[QuickSaveButton] Save1 panel reference is null. UI will refresh next time the panel is enabled.");
        }
    }
}
