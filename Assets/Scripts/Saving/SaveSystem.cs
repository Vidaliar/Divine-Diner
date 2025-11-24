using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.VersionControl;
using UnityEngine;

public class SaveSystem : MonoBehaviour, ISaveSystem
{
    [Header("StateProvider")]
    public MonoBehaviour stateProviderBehaviour; 
    private IStateProvider provider;

    [Header("root path")]
    public string rootFolder = "Saves";

    [Header("PNG settings")]
    public bool captureViaCamera = false;  // false=screenshot£»true=remove UI
    public Camera captureCamera;           // put the camera that need to shot the PNG
    public int thumbWidth = 512;
    public int thumbHeight = 288;

    [Header("UI to hide")]
    public GameObject[] uiToHideTemporarily;

    private void Awake()
    {
        provider = stateProviderBehaviour as IStateProvider;
        if (stateProviderBehaviour != null && provider == null)
            Debug.LogError("[SaveSystem] stateProviderBehaviour not initialize IStateProvider");
    }

    // ========== ISaveSystem ==========
    public bool HasSave(string profile, int slotIndex) => File.Exists(JsonPath(profile, slotIndex));

    public SaveMeta GetMeta(string profile, int slotIndex)
    {
        string path = JsonPath(profile, slotIndex);
        if (!File.Exists(path)) return default;

        try
        {
            string json = File.ReadAllText(path);
            var file = JsonUtility.FromJson<SaveFile>(json);
            return file.meta;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveSystem] ¶ÁÈ¡ Meta Ê§°Ü£º{path}\n{e}");
            return default;
        }
    }

    public string GetThumbnailPath(string profile, int slotIndex)
    {
        string p = ThumbPath(profile, slotIndex);
        return File.Exists(p) ? p : string.Empty;
    }

    public void SaveCurrentToSlot(string profile, int slotIndex, string title = null, string subtitle = null)
    {
        StartCoroutine(SaveRoutine(profile, slotIndex, title, subtitle));
    }

    public void Load(string profile, int slotIndex)
    {
        StartCoroutine(LoadRoutine(profile, slotIndex));
    }

    public void Delete(string profile, int slotIndex)
    {
        string j = JsonPath(profile, slotIndex);
        string t = ThumbPath(profile, slotIndex);
        if (File.Exists(j)) File.Delete(j);
        if (File.Exists(t)) File.Delete(t);
    }

    // Save / Load functions
    private string RootDir => Path.Combine(Application.persistentDataPath, rootFolder);
    private string ProfileDir(string profile) => Path.Combine(RootDir, Sanitize(profile));
    private string JsonPath(string profile, int slot) => Path.Combine(ProfileDir(profile), $"slot{slot}.json");
    private string ThumbPath(string profile, int slot) => Path.Combine(ProfileDir(profile), $"thumb_slot{slot}.png");

    private string Sanitize(string s)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            s = s.Replace(c, '_');
        return string.IsNullOrEmpty(s) ? "Default" : s;
    }

    //private IEnumerator SaveRoutine(string profile, int slotIndex, string title, string subtitle)
    //{
        
    //}

    private IEnumerator LoadRoutine(string profile, int slotIndex)
    {
        string path = JsonPath(profile, slotIndex);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveSystem] save not exist: {path}");
            yield break;
        }

        SaveFile file = null;
        try
        {
            string json = File.ReadAllText(path);
            file = JsonUtility.FromJson<SaveFile>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] save load failed: {path}\n{e}");
            yield break;
        }

        if (provider == null)
        {
            Debug.LogWarning("[SaveSystem] IStateProvider not set, unable to load");
            yield break;
        }

        // restore day
        yield return provider.Apply(file.data);

        // resume the time/audio
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}
