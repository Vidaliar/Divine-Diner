using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    
}
