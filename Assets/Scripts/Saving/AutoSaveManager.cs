using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// AutoSaveManager
/// --------------------------------------------------
/// Purpose:
///   Encapsulates the logic for automatic saving. When the game reaches
///   a specific episode, this component calls SaveSystem to create or
///   overwrite a dedicated auto-save slot.
///
/// Basic usage:
///   1. In your scene, create an empty GameObject, e.g. "AutoSaveManager".
///   2. Attach this AutoSaveManager script to that GameObject.
///   3. In the inspector:
///        - Save System     : assign the SaveSystem component in the scene.
///        - Auto Profile    : e.g. "AutoSave" (profile used only for auto saves).
///        - Auto Slot Index : e.g. 0 (single auto-save slot that is always overwritten).
///        - Enable AutoSave : check this to enable auto-save behavior.
///
///   4. In your story / episode controller script:
///        - Make sure StateProvider.currentDay and currentEpisode
///          are updated correctly when you enter a new episode.
///        - At the moment you decide "we have reached this episode",
///          call:
///             AutoSaveManager.Instance.AutoSaveNow();
///
///   5. If you want a UI button to load the auto-save:
///        - You can call:
///             saveSystem.Load("AutoSave", 0);
///        - Or reuse your SaveMenus script with:
///             profileName = "AutoSave";
///             slotIndex   = 0;
///
/// Notes:
///   - Auto-save uses the same SaveSystem pipeline as manual saves:
///       StateProvider.Capture() -> SaveData -> JSON + PNG.
///   - It does not overwrite your manual save slots if you keep the
///       auto-save profile name separate (e.g. "AutoSave" vs "Default").
/// --------------------------------------------------
/// </summary>
public class AutoSaveManager : MonoBehaviour
{
    public static AutoSaveManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("SaveSystem instance that writes save files and thumbnails.")]
    public SaveSystem saveSystem;

    [Header("Auto save config")]
    [Tooltip("Profile name used exclusively for automatic saves.")]
    public string autoProfileName = "AutoSave";

    [Tooltip("Slot index used exclusively for automatic saves.")]
    public int autoSlotIndex = 0;

    [Tooltip("Global toggle for enabling or disabling auto-save logic.")]
    public bool enableAutoSave = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Perform an auto-save immediately using the configured profile and slot.
    /// Intended to be called when the game has just arrived at a new episode
    /// and StateProvider already holds the correct day/episode/affection values.
    /// </summary>
    public void AutoSaveNow()
    {
        if (!enableAutoSave)
        {
            Debug.Log("[AutoSaveManager] Auto-save is disabled. Skipping save.");
            return;
        }

        if (saveSystem == null)
        {
            Debug.LogWarning("[AutoSaveManager] SaveSystem reference is missing. Auto-save aborted.");
            return;
        }

        saveSystem.SaveCurrentToSlot(autoProfileName, autoSlotIndex);
        Debug.Log($"[AutoSaveManager] Auto-saved to {autoProfileName}/slot{autoSlotIndex}");
    }

    /// <summary>
    /// Optional helper method:
    /// If you prefer a single call from your episode flow, you can use:
    ///     AutoSaveManager.Instance.OnReachEpisode(day, episode);
    /// This method assumes that elsewhere in your code you will set
    /// the actual StateProvider fields (currentDay/currentEpisode)
    /// before or after calling this, depending on your architecture.
    /// For many projects, it is simpler to just set progress in your
    /// own controller, then call AutoSaveNow() directly.
    /// </summary>
    public void OnReachEpisode(int day, int episode)
    {
        if (!enableAutoSave || saveSystem == null)
            return;

        AutoSaveNow();
    }
}
