using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// QuickSaveButton
/// --------------------------------------------------
/// Purpose:
///   Provides a simple entry point for "Quick Save" UI or input.
///   When invoked, it writes the current game state into a fixed
///   save slot (by default: profile "Default", slotIndex 0),
///   which in your UI is the "Save 1" slot.
///
/// Basic usage:
///   1. Create an empty GameObject in your scene, or use an existing
///      UI button GameObject.
///   2. Attach this QuickSaveButton script to that GameObject.
///   3. In the inspector:
///        - Save System : assign the SaveSystem component in the scene.
///        - Save 1 Panel (optional) : assign the SaveMenus panel that
///          represents "Save 1" (Default / slot 0). If assigned, it will
///          be refreshed after a quick save when the menu is open.
///        - Profile Name : usually "Default".
///        - Slot Index   : 0 (this is your Save 1 slot).
///
///   4. To hook it to a UI Button:
///        - Select your "Quick Save" Button in the hierarchy.
///        - In the OnClick list, add a new entry:
///            * Drag the GameObject that has QuickSaveButton into the slot.
///            * Choose QuickSaveButton.QuickSave() as the function.
///        - Now clicking that button will perform a quick save.
///
///   5. If you want to trigger quick save from code:
///        - Call QuickSaveButton.QuickSave() from your own scripts,
///          for example when player presses a specific key.
/// --------------------------------------------------
/// </summary>
public class QuickSaveButton : MonoBehaviour
{
    [Header("References")]
    public SaveSystem saveSystem;
    public SaveMenus save1Panel;

    [Header("Target slot")]
    public string profileName = "Default";
    public int slotIndex = 0;

    /// <summary>
    /// Perform a quick save to the configured profile/slot.
    /// Intended to be called from a UI Button OnClick or other input.
    /// </summary>
    public void QuickSave()
    {
        if (saveSystem == null)
        {
            Debug.LogWarning("[QuickSaveButton] SaveSystem reference is missing. Quick save aborted.");
            return;
        }

        saveSystem.SaveCurrentToSlot(profileName, slotIndex);
        Debug.Log($"[QuickSaveButton] Quick saved to {profileName}/slot{slotIndex}");

        if (save1Panel != null)
        {
            save1Panel.RefreshUI();
        }
    }
}
