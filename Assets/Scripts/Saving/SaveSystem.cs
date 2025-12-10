using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour, ISaveSystem
{
    // ===========================
    // HOW TO USE (READ FIRST)
    // ===========================
    // 1) Put this component on an object in your bootstrap/start scene.
    //    (Optionally enable "makePersistent" so it survives scene changes.)
    //
    // 2) You do NOT need to drag references in Inspector:
    //    - The system will auto-find any MonoBehaviour that implements IStateProvider.
    //      (You can still assign "stateProviderBehaviour" manually if you prefer.)
    //
    // 3) Scene routing for Load():
    //    - Preferred: Fill "sceneMap" with (day, episode) -> sceneName.
    //    - Fallback: If not found in the list, it tries "Day{day}_Episode{episode}".
    //    - Make sure target scenes are added to Build Settings > Scenes In Build.
    //
    // 4) Thumbnails:
    //    - If "captureViaCamera" = true: uses the given camera to render a thumbnail.
    //      (Exclude UI via Culling Mask of that camera.)
    //    - If "captureViaCamera" = false: takes a full-screen shot and temporarily
    //      hides "uiToHideTemporarily" gameObjects to avoid capturing UI.
    //
    // 5) API:
    //    - SaveCurrentToSlot(profile, slotIndex, title?, subtitle?)
    //    - Load(profile, slotIndex)
    //    - Delete(profile, slotIndex)
    //    - HasSave(profile, slotIndex)
    //    - GetMeta(profile, slotIndex) / GetThumbnailPath(profile, slotIndex)
    //
    // 6) Files:
    //    - JSON path:   {persistentDataPath}/{rootFolder}/{profile}/slot{N}.json
    //    - PNG path:    {persistentDataPath}/{rootFolder}/{profile}/thumb_slot{N}.png
    //
    // 7) Extensibility:
    //    - To save more fields, add them to SaveData and implement them in your
    //      IStateProvider.Capture()/Apply(). This SaveSystem does NOT need changes.

    [Header("Auto Wiring")]
    [Tooltip("Optional. A MonoBehaviour that implements IStateProvider. Can be left empty.")]
    [SerializeField] private MonoBehaviour stateProviderBehaviour;
    [Tooltip("If true, the system will automatically search for an IStateProvider in the scene.")]
    [SerializeField] private bool autoFindProvider = true;
    [Tooltip("If true, SaveSystem will not be destroyed across scene loads.")]
    [SerializeField] private bool makePersistent = true;

    private IStateProvider _provider;
    private bool _isSaving;
    private bool _isLoading;

    [Header("Root Path")]
    [Tooltip("Root folder under Application.persistentDataPath.")]
    public string rootFolder = "Saves";

    [Header("PNG Settings")]
    [Tooltip("If true, use the given Camera to render a thumbnail; otherwise capture full screen.")]
    public bool captureViaCamera = false;
    [Tooltip("Camera used when captureViaCamera = true. UI should be excluded via Culling Mask.")]
    public Camera captureCamera;
    [Tooltip("Thumbnail width in pixels.")]
    public int thumbWidth = 300;
    [Tooltip("Thumbnail height in pixels.")]
    public int thumbHeight = 169; // 16:9-ish

    [Header("UI to Hide (Full-screen only)")]
    [Tooltip("Temporarily hidden during full-screen capture to avoid UI in thumbnails.")]
    public List<GameObject> uiToHideTemporarily = new List<GameObject>();

    [Header("Scene Routing")]
    [Tooltip("Optional mapping for (day, episode) -> sceneName. If not found, fallback to Day{d}_Episode{e}.")]
    [SerializeField] private List<DayEpisodeScene> sceneMap = new List<DayEpisodeScene>();

    [Serializable]
    public struct DayEpisodeScene
    {
        public int day;
        public int episode;
        public string sceneName; // must exist in Build Settings
    }

    // ------------- Unity lifecycle -------------
    private void Awake()
    {
        if (makePersistent)
            DontDestroyOnLoad(gameObject);
    }

    // ------------- Provider resolving -------------
    private IStateProvider Provider
    {
        get
        {
            if (_provider != null) return _provider;

            // 1) If manually assigned and implements IStateProvider
            if (stateProviderBehaviour is IStateProvider p1)
                return _provider = p1;

            if (!autoFindProvider) return null;

            // 2) Auto-find any MonoBehaviour implementing IStateProvider (including inactive)
#if UNITY_2022_2_OR_NEWER
            var mbs = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var mbs = FindObjectsOfType<MonoBehaviour>(true);
#endif
            foreach (var mb in mbs)
            {
                if (mb is IStateProvider p)
                {
                    _provider = p;
                    break;
                }
            }
            return _provider;
        }
    }

    // ------------- Public API (ISaveSystem) -------------
    public bool HasSave(string profile, int slotIndex)
    {
        return File.Exists(JsonPath(profile, slotIndex));
    }

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
            Debug.LogError($"[SaveSystem] GetMeta failed: {path}\n{e}");
            return default;
        }
    }

    public string GetThumbnailPath(string profile, int slotIndex)
    {
        return ThumbPath(profile, slotIndex);
    }

    public void SaveCurrentToSlot(string profile, int slotIndex, string title = null, string subtitle = null)
    {
        if (_isSaving)
        {
            Debug.LogWarning("[SaveSystem] Save is already running.");
            return;
        }
        StartCoroutine(SaveRoutine(profile, slotIndex, title, subtitle));
    }

    public void Load(string profile, int slotIndex)
    {
        if (_isLoading)
        {
            Debug.LogWarning("[SaveSystem] Load is already running.");
            return;
        }
        StartCoroutine(LoadRoutine(profile, slotIndex));
    }

    public void Delete(string profile, int slotIndex)
    {
        string json = JsonPath(profile, slotIndex);
        string png = ThumbPath(profile, slotIndex);

        try
        {
            if (File.Exists(json)) File.Delete(json);
            if (File.Exists(png)) File.Delete(png);

            // Optional: delete empty profile folder
            var folder = ProfileFolder(profile);
            if (Directory.Exists(folder) && Directory.GetFiles(folder).Length == 0 && Directory.GetDirectories(folder).Length == 0)
            {
                Directory.Delete(folder);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Delete failed: {profile}/slot{slotIndex}\n{e}");
        }
    }

    // ------------- Save / Load Coroutines -------------
    private IEnumerator SaveRoutine(string profile, int slotIndex, string title, string subtitle)
    {
        _isSaving = true;

        // Resolve provider
        var p = Provider;
        if (p == null)
        {
            Debug.LogError("[SaveSystem] No IStateProvider found for Capture(). Aborting save.");
            _isSaving = false;
            yield break;
        }

        // Capture data from provider
        SaveData data = null;
        try
        {
            data = p.Capture();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Provider.Capture failed:\n{e}");
            _isSaving = false;
            yield break;
        }

        // Prepare meta
        if (string.IsNullOrEmpty(title))
        {
            // Example default title using day: "Day 1"
            title = $"Day {data.day}";
        }
        if (string.IsNullOrEmpty(subtitle))
        {
            // Example default subtitle using local time (UI can localize further)
            subtitle = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

        // Ensure folder exists
        string folder = ProfileFolder(profile);
        EnsureDirectory(folder);

        string jsonPath = JsonPath(profile, slotIndex);
        string thumbPath = ThumbPath(profile, slotIndex);

        // Prepare thumbnail (hide UI if full-screen)
        var hidden = new List<GameObject>();
        if (!captureViaCamera && uiToHideTemporarily != null)
        {
            foreach (var go in uiToHideTemporarily)
            {
                if (go && go.activeSelf) { go.SetActive(false); hidden.Add(go); }
            }
        }

        if (captureViaCamera)
        {
            if (captureCamera == null)
            {
                Debug.LogWarning("[SaveSystem] captureViaCamera=true but captureCamera is null. Fallback to screen.");
                yield return ScreenShooter.CaptureScreen(thumbPath, thumbWidth, thumbHeight);
            }
            else
            {
                var tex = ScreenShooter.CaptureCameraToTexture(captureCamera, thumbWidth, thumbHeight);
                ScreenShooter.WritePNG(tex, thumbPath);
                Destroy(tex);
                yield return null;
            }
        }
        else
        {
            // Capture entire screen to file
            yield return ScreenShooter.CaptureScreen(thumbPath, thumbWidth, thumbHeight);
        }

        // Restore UI
        foreach (var go in hidden) if (go) go.SetActive(true);

        // Compose SaveFile and write JSON
        var saveFile = new SaveFile
        {
            meta = new SaveMeta
            {
                title = title,
                subtitle = subtitle,
                thumbnailPath = thumbPath,
                timeISO = DateTime.UtcNow.ToString("o"),
            },
            data = data
        };

        string json = JsonUtility.ToJson(saveFile, true);
        try
        {
            File.WriteAllText(jsonPath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Write JSON failed: {jsonPath}\n{e}");
        }

        _isSaving = false;
        yield return null;
    }

    private IEnumerator LoadRoutine(string profile, int slotIndex)
    {
        _isLoading = true;

        string path = JsonPath(profile, slotIndex);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveSystem] Save not found: {path}");
            _isLoading = false;
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
            Debug.LogError($"[SaveSystem] Read JSON failed: {path}\n{e}");
            _isLoading = false;
            yield break;
        }

        if (file == null || file.data == null)
        {
            Debug.LogWarning("[SaveSystem] Save file has no data; aborting load.");
            _isLoading = false;
            yield break;
        }

        // Resolve target scene by (day, episode)
        string targetScene = ResolveSceneName(file.data);

        // Make sure the game is unpaused BEFORE changing scene
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("[SaveSystem] ResolveSceneName returned empty; staying in current scene.");
            _isLoading = false;
            yield break;
        }

        var op = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Single);
        if (op != null) yield return op;
        else Debug.LogError($"[SaveSystem] LoadSceneAsync failed: {targetScene}");

        // Optional: if you later add more fields to SaveData that require applying AFTER
        // the target scene is loaded, you can re-resolve Provider here and call Apply:
        // var p = Provider; if (p != null) yield return p.Apply(file.data);

        _isLoading = false;
    }

    // ------------- Helpers -------------
    private string ResolveSceneName(SaveData data)
    {
        if (data == null) return null;

        // Try explicit mapping first
        if (sceneMap != null)
        {
            for (int i = 0; i < sceneMap.Count; i++)
            {
                var m = sceneMap[i];
                if (m.day == data.day && m.episode == data.episode && !string.IsNullOrEmpty(m.sceneName))
                    return m.sceneName;
            }
        }
        // Fallback to naming convention: Day{day}_Episode{episode}
        return $"Day{data.day}_Episode{data.episode}";
    }

    private string ProfileFolder(string profile)
    {
        // {persistentDataPath}/{rootFolder}/{profile}
        string baseDir = Application.persistentDataPath;
        return Path.Combine(baseDir, rootFolder, profile ?? "Default");
    }

    private string JsonPath(string profile, int slotIndex)
    {
        return Path.Combine(ProfileFolder(profile), $"slot{slotIndex}.json");
    }

    private string ThumbPath(string profile, int slotIndex)
    {
        return Path.Combine(ProfileFolder(profile), $"thumb_slot{slotIndex}.png");
    }

    private static void EnsureDirectory(string fullOrDirPath)
    {
        if (string.IsNullOrEmpty(fullOrDirPath)) return;
        string dir = fullOrDirPath;
        // If a file path was passed in, get its directory.
        if (Path.HasExtension(fullOrDirPath))
            dir = Path.GetDirectoryName(fullOrDirPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }
}
