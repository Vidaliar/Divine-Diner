using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
================================================================================
SettingsManager.cs — Global manager for loading, saving and applying settings
================================================================================

IMPLEMENTATION GUIDE (How to use)

1) Scene Setup
   - Create a single empty GameObject in your startup scene
     (e.g., "SystemRoot" or "SettingsManagerRoot").
   - Attach this script to that GameObject.
   - This object will:
       * Enforce a singleton (SettingsManager.Instance).
       * Persist across scenes via DontDestroyOnLoad.

2) Responsibilities
   - Holds the current active SettingsData ("Current").
   - Loads settings from disk when the game boots (using PlayerPrefs + JSON).
   - Saves settings back to disk when requested.
   - Applies settings to the game (Audio, Screen, etc.).
   - Provides a single entry point for other systems:
       SettingsManager.Instance.Current

3) Typical Flow
   - On game startup:
       - Awake() is called.
       - LoadFromDisk() loads saved JSON or uses default settings.
       - ApplyCurrent() applies the loaded settings to the game.
   - When user changes settings and clicks "Save & Exit" in SettingsMenuUI:
       - SettingsMenuUI calls:
           SettingsManager.Instance.OverrideCurrent(editingData);
           SettingsManager.Instance.SaveToDisk();
           SettingsManager.Instance.ApplyCurrent();

4) Replacing PlayerPrefs
   - You can swap out PlayerPrefs and JSON with your own save system:
       * Modify LoadFromDisk() to read from your JSON save files.
       * Modify SaveToDisk() to write to your save system.
   - The rest of the code (SettingsMenuUI, etc.) can remain unchanged.

NOTES
- Keep this manager unique in the scene (singleton).
- ApplyCurrent() is intentionally minimal and should be extended when new
  settings are added.
================================================================================
*/


public class SettingsManager : MonoBehaviour
{
    // Singleton instance
    public static SettingsManager Instance { get; private set; }

    // The currently active settings that the game is using
    public SettingsData Current { get; private set; } = new SettingsData();

    // PlayerPrefs key for the JSON blob
    private const string PlayerPrefsKey = "GameSettings";

    private void Awake()
    {
        // Basic singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved settings or default values
        LoadFromDisk();

        // Apply loaded settings to the game
        ApplyCurrent();

    }

    // Load settings from disk (PlayerPrefs + JSON)
    public void LoadFromDisk()
    {
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            string json = PlayerPrefs.GetString(PlayerPrefsKey);

            if (!string.IsNullOrEmpty(json))
            {
                SettingsData loaded = JsonUtility.FromJson<SettingsData>(json);
                if (loaded != null)
                {
                    Current = loaded;
                    return;
                }
            }
        }

        // Fallback: use default settings
        Current = new SettingsData();
    }

    // Save current settings to disk (PlayerPrefs + JSON)
    public void SaveToDisk()
    {
        string json = JsonUtility.ToJson(Current);
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
    }

    public void ApplyCurrent()
    {
        // ===== Placeholder implementation =====
        // Example: apply volume
        // AudioListener.volume = Current.masterVolume;

        // Example: apply full screen setting
        // Screen.fullScreen = Current.isFullScreen;

        // TBD: add things here
    }

    // Replace the current active settings with a new copy
    public void OverrideCurrent(SettingsData newData)
    {
        if (newData == null)
        {
            Debug.LogWarning("SettingsManager.OverrideCurrent called with null data.");
            return;
        }

        Current = newData.Clone();
    }


}
