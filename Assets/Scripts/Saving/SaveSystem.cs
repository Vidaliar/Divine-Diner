using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour, ISaveSystem
{
    [Header("StateProvider")]
    public MonoBehaviour stateProviderBehaviour; 
    private IStateProvider provider;

    [Header("root path")]
    public string rootFolder = "Saves";

    [Header("PNG settings")]
    public bool captureViaCamera = false;  // false=screenshot��true=remove UI
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
            Debug.LogWarning($"[SaveSystem] ��ȡ Meta ʧ�ܣ�{path}\n{e}");
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

    private IEnumerator SaveRoutine(string profile, int slotIndex, string title, string subtitle)
    {
        if (provider == null)
        {
            Debug.LogWarning("[SaveSystem] not set IStateProvider, unable to save");
            yield break;
        }

        // make sure the path of profile
        Directory.CreateDirectory(ProfileDir(profile));

        // get data
        var data = provider.Capture();

        // generate the PNG
        string thumbPath = ThumbPath(profile, slotIndex);
        var hidden = new System.Collections.Generic.List<GameObject>();

        if (!captureViaCamera && uiToHideTemporarily != null)
        {
            foreach (var go in uiToHideTemporarily)
            {
                if (go && go.activeSelf) { go.SetActive(false); hidden.Add(go); }
            }
        }

        if (captureViaCamera)
        {
            if (captureCamera == null) Debug.LogWarning("[SaveSystem] captureViaCamera=true ��δָ�� captureCamera");
            var tex = ScreenShooter.CaptureCameraToTexture(
                captureCamera ? captureCamera : Camera.main,
                thumbWidth, thumbHeight
            );
            ScreenShooter.WritePNG(tex, thumbPath);
            Destroy(tex);
            yield return null;
        }
        else
        {
            // ScreenShooter make sure the path again
            yield return ScreenShooter.CaptureScreen(thumbPath, thumbWidth, thumbHeight);
        }

        foreach (var go in hidden) go.SetActive(true);

        // write JSON
        var now = DateTime.Now;
        var meta = new SaveMeta
        {
            title = string.IsNullOrEmpty(title) ? $"Day {data.day} Ep {data.episode}" : title,
            subtitle = string.IsNullOrEmpty(subtitle) ? now.ToString("yyyy-MM-dd HH:mm") : subtitle,
            thumbnailPath = thumbPath,
            timeISO = now.ToString("o")
        };

        var file = new SaveFile { meta = meta, data = data };
        string json = JsonUtility.ToJson(file, prettyPrint: true);

        Directory.CreateDirectory(ProfileDir(profile));
        File.WriteAllText(JsonPath(profile, slotIndex), json);

        Debug.Log($"[SaveSystem] Saved: {profile}/slot{slotIndex} (day={data.day}, ep={data.episode})");
    }

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

        if (file == null || file.data == null)
        {
            Debug.LogWarning("[SaveSystem] save file is empty or corrupted.");
            yield break;
        }

        if (string.IsNullOrEmpty(file.data.sceneName))
        {
            Debug.LogWarning("[SaveSystem] save file has no sceneName.");
            yield break;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        string targetScene = file.data.sceneName;

        // If not in the correct scene, switch scene first.
        // The actual data restore will happen in the target scene
        // through AutoLoadOnSceneStart.
        if (currentScene != targetScene)
        {
            Debug.Log($"[SaveSystem] Scene mismatch. Current={currentScene}, Target={targetScene}. Switching scene first.");
            GlobalLoadContext.Request(profile, slotIndex);
            SceneManager.LoadScene(targetScene);
            yield break;
        }

        if (provider == null)
        {
            Debug.LogWarning("[SaveSystem] IStateProvider not set, unable to load");
            yield break;
        }

        // already in the correct scene, apply data now.
        yield return provider.Apply(file.data);

        Debug.Log($"[SaveSystem] Apply finished in scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

        YarnRestoreController yarnRestoreController = FindFirstObjectByType<YarnRestoreController>();

        if (yarnRestoreController == null)
        {
            yarnRestoreController = FindObjectOfType<YarnRestoreController>();
        }

        if (yarnRestoreController != null)
        {
            Debug.Log($"[SaveSystem] Found YarnRestoreController on object: {yarnRestoreController.gameObject.name}");
            yarnRestoreController.RestoreFromSavedState();
            Debug.Log("[SaveSystem] Called RestoreFromSavedState()");
        }
        else
        {
            Debug.LogWarning("[SaveSystem] YarnRestoreController not found in current scene.");
        }

        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public SaveFile ReadSaveFile(string profile, int slotIndex)
    {
        string path = JsonPath(profile, slotIndex);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveSystem] ReadSaveFile: {path} does not exist.");
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveFile>(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveSystem] ReadSaveFile: failed to parse json at {path}\n{e}");
            return null;
        }
    }


}
