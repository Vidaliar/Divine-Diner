using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
[DefaultExecutionOrder(-1000)]
public class AudioSettings : MonoBehaviour
{
    public static AudioSettings Instance { get; private set; }

    [Header("Unity Audio (Optional AudioMixer)")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterVolumeParam = "MasterVol";
    [SerializeField] private string bgmVolumeParam = "BgmVol";

    [Header("No Mixer Mode (BGM AudioSource)")]
    [SerializeField] private AudioSource bgmSource;

    [Header("Auto Resolve")]
    [SerializeField] private bool autoFindBgmByName = true;
    [SerializeField] private string bgmObjectName = "BGM";

    [Header("Mapping")]
    [Range(-80f, -10f)] public float minDb = -60f;
    [Range(0.1f, 5f)] public float curve = 2.2f;

    [Header("PlayerPrefs Keys")]
    public string keyMasterVol = "aud_master_vol";
    public string keyBgmVol = "aud_bgm_vol";
    public string keySfxVol = "aud_sfx_vol";
    public string keyMuteAll = "aud_mute_all";
    public string keyMuteBgm = "aud_mute_bgm";
    public string keyMuteSfx = "aud_mute_sfx";

    public float MasterVol01 { get; private set; } = 1f;
    public float BgmVol01 { get; private set; } = 1f;
    public float SfxVol01 { get; private set; } = 1f; // placeholder

    public bool MuteAll { get; private set; }
    public bool MuteBgm { get; private set; }
    public bool MuteSfx { get; private set; } // placeholder

    private bool _warnedSfxPlaceholder;
    private bool _warnedBgmSourceMissing;
    private bool _warnedMasterParamMissing;
    private bool _warnedBgmParamMissing;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapAfterSceneLoad()
    {
        if (Instance != null) return;

#if UNITY_2022_2_OR_NEWER
        var existing = Object.FindFirstObjectByType<AudioSettings>();
#else
        var existing = Object.FindObjectOfType<AudioSettings>();
#endif
        if (existing != null) return;

        var go = new GameObject("AudioSettings (Auto)");
        go.AddComponent<AudioSettings>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        LoadFromPrefs();
        TryResolveBgmSource();
        ApplyAll();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryResolveBgmSource();
        ApplyAll();
    }

    private void TryResolveBgmSource()
    {
        if (audioMixer != null) return;
        if (bgmSource != null) return;

        if (!autoFindBgmByName) return;

        var go = GameObject.Find(bgmObjectName);
        if (go != null) bgmSource = go.GetComponent<AudioSource>();

        if (bgmSource == null && !_warnedBgmSourceMissing)
        {
            _warnedBgmSourceMissing = true;
            Debug.LogWarning($"[AudioSettings] No AudioMixer and bgmSource not set. " +
                             $"Tried find GameObject '{bgmObjectName}' but failed. ");
        }
    }

    private void LoadFromPrefs()
    {
        MasterVol01 = PlayerPrefs.GetFloat(keyMasterVol, 1f);
        BgmVol01 = PlayerPrefs.GetFloat(keyBgmVol, 1f);
        SfxVol01 = PlayerPrefs.GetFloat(keySfxVol, 1f);

        MuteAll = PlayerPrefs.GetInt(keyMuteAll, 0) == 1;
        MuteBgm = PlayerPrefs.GetInt(keyMuteBgm, 0) == 1;
        MuteSfx = PlayerPrefs.GetInt(keyMuteSfx, 0) == 1;
    }

    private void SaveToPrefs()
    {
        PlayerPrefs.SetFloat(keyMasterVol, MasterVol01);
        PlayerPrefs.SetFloat(keyBgmVol, BgmVol01);
        PlayerPrefs.SetFloat(keySfxVol, SfxVol01);

        PlayerPrefs.SetInt(keyMuteAll, MuteAll ? 1 : 0);
        PlayerPrefs.SetInt(keyMuteBgm, MuteBgm ? 1 : 0);
        PlayerPrefs.SetInt(keyMuteSfx, MuteSfx ? 1 : 0);

        PlayerPrefs.Save();
    }

    private float Shape01(float v01)
    {
        v01 = Mathf.Clamp01(v01);
        return Mathf.Pow(v01, curve);
    }

    private float Slider01ToDb(float v01)
    {
        float shaped = Shape01(v01);
        return Mathf.Lerp(minDb, 0f, shaped);
    }

    private void ApplyAll()
    {
        if (audioMixer != null) ApplyMixer();
        else ApplyNoMixer();
    }

    private void ApplyMixer()
    {
        float masterDb = MuteAll ? minDb : Slider01ToDb(MasterVol01);
        if (!audioMixer.SetFloat(masterVolumeParam, masterDb) && !_warnedMasterParamMissing)
        {
            _warnedMasterParamMissing = true;
            Debug.LogWarning($"[AudioSettings] AudioMixer missing param '{masterVolumeParam}'.");
        }

        float bgmDb = MuteBgm ? minDb : Slider01ToDb(BgmVol01);
        if (!audioMixer.SetFloat(bgmVolumeParam, bgmDb) && !_warnedBgmParamMissing)
        {
            _warnedBgmParamMissing = true;
            Debug.LogWarning($"[AudioSettings] AudioMixer missing param '{bgmVolumeParam}'.");
        }
    }

    private void ApplyNoMixer()
    {
        AudioListener.volume = MuteAll ? 0f : Shape01(MasterVol01);

        if (bgmSource != null)
        {
            bgmSource.mute = MuteBgm;
            bgmSource.volume = Shape01(BgmVol01);
        }
    }

    private void WarnSfxPlaceholderOnce()
    {
        if (_warnedSfxPlaceholder) return;
        _warnedSfxPlaceholder = true;
        Debug.LogWarning("[AudioSettings] SFX settings are placeholder (FMOD not wired).");
    }

    public void SetMasterVolume01(float v01) { MasterVol01 = Mathf.Clamp01(v01); SaveToPrefs(); ApplyAll(); }
    public void SetBgmVolume01(float v01) { BgmVol01 = Mathf.Clamp01(v01); SaveToPrefs(); ApplyAll(); }
    public void SetMuteAll(bool mute) { MuteAll = mute; SaveToPrefs(); ApplyAll(); }
    public void SetMuteBgm(bool mute) { MuteBgm = mute; SaveToPrefs(); ApplyAll(); }
    public void ToggleMuteAll() => SetMuteAll(!MuteAll);
    public void ToggleMuteBgm() => SetMuteBgm(!MuteBgm);

    // ===== SFX:placeholder =====
    public void SetSfxVolume01(float v01) { SfxVol01 = Mathf.Clamp01(v01); SaveToPrefs(); WarnSfxPlaceholderOnce(); }
    public void SetMuteSfx(bool mute) { MuteSfx = mute; SaveToPrefs(); WarnSfxPlaceholderOnce(); }
    public void ToggleMuteSfx() => SetMuteSfx(!MuteSfx);

    public void SetBgmSource(AudioSource source)
    {
        bgmSource = source;
        ApplyAll();
    }
}
