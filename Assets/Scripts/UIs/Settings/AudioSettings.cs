using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/*
SUMMARY (NO MIXER + FMOD SFX BUS)
- This script does NOT use Unity AudioMixer.
- Total (Unity) volume:
    Uses AudioListener.volume (affects all Unity audio, including BGM).
- BGM (Unity) volume:
    Uses a dedicated Unity AudioSource (bgmSource). BGM effective loudness is:
      effectiveBgm = AudioListener.volume * bgmSource.volume
- SFX (FMOD) volume:
    Uses an FMOD Bus (e.g., "bus:/SFX").
    Requirements in FMOD Studio:
      1) Create an SFX Bus under Master in the Mixer (e.g., SFX).
      2) Route all SFX events output to that Bus.
      3) Build Banks and refresh banks in Unity (FMOD > Refresh Banks).
      4) Copy the Bus path (Copy Path) and paste it into 'sfxBusPath' in Inspector.

- Safety:
    If the FMOD bus path is wrong / not found, FMOD control is disabled for the session,
    and Unity (Total/BGM) controls continue working normally.
*/

public class AudioSettings : MonoBehaviour
{
    public static AudioSettings Instance { get; private set; }

    [Header("Unity BGM (AudioSource)")]
    [Tooltip("Assign your BGM AudioSource here for best reliability. If empty, the script will try to find a GameObject named 'BGM' at runtime.")]
    [SerializeField] private AudioSource bgmSource;

    [Header("FMOD SFX (Bus)")]
    [Tooltip("FMOD SFX bus path. Example: bus:/SFX (use Copy Path in FMOD Studio).")]
    [SerializeField] private string sfxBusPath = "bus:/SFX";

    [Tooltip("If disabled, FMOD SFX volume/mute will not be applied at all.")]
    [SerializeField] private bool enableFmodSfx = true;

    [Header("Perceptual Mapping")]
    [Tooltip("Perceptual curve for slider values (0..1). Larger means finer control at low volume.")]
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
    public float SfxVol01 { get; private set; } = 1f;

    public bool MuteAll { get; private set; }
    public bool MuteBgm { get; private set; }
    public bool MuteSfx { get; private set; }

    private Bus _sfxBus;
    private bool _fmodSfxReady;
    private bool _warnedFmodOnce;
    private bool _warnedBgmOnce;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadFromPrefs();

        // Acquire references safely (never throw).
        EnsureBgmSource();
        AcquireFmodBusSafe();

        ApplyAll();
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

    private void EnsureBgmSource()
    {
        if (bgmSource != null) return;

        // Common fallback: a GameObject named "BGM" that holds the AudioSource.
        var go = GameObject.Find("BGM");
        if (go != null)
            bgmSource = go.GetComponent<AudioSource>();
    }

    private void AcquireFmodBusSafe()
    {
        _fmodSfxReady = false;
        if (!enableFmodSfx) return;

        if (string.IsNullOrWhiteSpace(sfxBusPath))
            return;

        try
        {
            _sfxBus = RuntimeManager.GetBus(sfxBusPath);
            _fmodSfxReady = _sfxBus.isValid();

            if (!_fmodSfxReady && !_warnedFmodOnce)
            {
                Debug.LogWarning($"[AudioSettings] FMOD bus invalid: '{sfxBusPath}'. FMOD SFX control disabled for this session.");
                _warnedFmodOnce = true;
                enableFmodSfx = false; // stop spamming
            }
        }
        catch (System.Exception e)
        {
            if (!_warnedFmodOnce)
            {
                Debug.LogWarning($"[AudioSettings] FMOD bus not found: '{sfxBusPath}'. " +
                                 $"FMOD SFX control disabled for this session. Details: {e.GetType().Name}");
                _warnedFmodOnce = true;
            }
            enableFmodSfx = false; // stop spamming
        }
    }

    private void ApplyAll()
    {
        ApplyUnityTotalAndBgm();
        ApplyFmodSfxSafe();
    }

    private void ApplyUnityTotalAndBgm()
    {
        // Total (Unity) volume
        AudioListener.volume = MuteAll ? 0f : Shape01(MasterVol01);

        // BGM volume/mute
        EnsureBgmSource();
        if (bgmSource != null)
        {
            bgmSource.mute = MuteBgm;
            bgmSource.volume = Shape01(BgmVol01);
        }
        else if (!_warnedBgmOnce)
        {
            Debug.LogWarning("[AudioSettings] BGM AudioSource not assigned and GameObject 'BGM' not found. BGM slider will not work until bgmSource is set.");
            _warnedBgmOnce = true;
        }
    }

    private void ApplyFmodSfxSafe()
    {
        if (!enableFmodSfx) return;

        if (!_fmodSfxReady || !_sfxBus.isValid())
        {
            AcquireFmodBusSafe();
            if (!enableFmodSfx || !_fmodSfxReady) return;
        }

        try
        {
            // Make Total feel global for FMOD too:
            // finalSfxVolume = Total * Sfx
            float total = Shape01(MasterVol01);
            float sfx = Shape01(SfxVol01);
            float finalVol = Mathf.Clamp01(total * sfx);

            bool finalMute = MuteAll || MuteSfx;

            _sfxBus.setVolume(finalVol);
            _sfxBus.setMute(finalMute);
        }
        catch
        {
            enableFmodSfx = false;
            if (!_warnedFmodOnce)
            {
                Debug.LogWarning("[AudioSettings] FMOD SFX apply failed; FMOD SFX control disabled for this session.");
                _warnedFmodOnce = true;
            }
        }
    }

    // ===== Public API for UI =====

    public void SetMasterVolume01(float v01)
    {
        MasterVol01 = Mathf.Clamp01(v01);
        SaveToPrefs();
        ApplyAll();
    }

    public void SetBgmVolume01(float v01)
    {
        BgmVol01 = Mathf.Clamp01(v01);
        SaveToPrefs();
        ApplyAll();
    }

    public void SetSfxVolume01(float v01)
    {
        SfxVol01 = Mathf.Clamp01(v01);
        SaveToPrefs();
        ApplyAll();
    }

    public void SetMuteAll(bool mute)
    {
        MuteAll = mute;
        SaveToPrefs();
        ApplyAll();
    }

    public void SetMuteBgm(bool mute)
    {
        MuteBgm = mute;
        SaveToPrefs();
        ApplyAll();
    }

    public void SetMuteSfx(bool mute)
    {
        MuteSfx = mute;
        SaveToPrefs();
        ApplyAll();
    }

    public void ToggleMuteAll() => SetMuteAll(!MuteAll);
    public void ToggleMuteBgm() => SetMuteBgm(!MuteBgm);
    public void ToggleMuteSfx() => SetMuteSfx(!MuteSfx);
}