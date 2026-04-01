using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Auto bind by child name")]
    public bool autoBindByName = true;

    [Header("Buttons")]
    public Button muteAllButton;
    public Button muteBgmButton;
    public Button muteSfxButton;

    [Header("Sliders")]
    public Slider totalSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("UX")]
    public bool autoUnmuteOnSliderChange = true;
    [Range(0.1f, 1f)] public float mutedAlpha = 0.45f;

    [Header("Init / Retry")]
    [Min(0f)] public float waitForInstanceTimeout = 1.0f;
    [Min(0f)] public float warnAfterSeconds = 0.2f;

    private AudioSettings _s;
    private bool _bound;
    private Coroutine _initCo;

    private void Awake()
    {
        if (autoBindByName) AutoBind();
        TryInitImmediate();
    }

    private void OnEnable()
    {
        StartInitRoutine();
    }

    private void OnDisable()
    {
        StopInitRoutine();
        UnbindOnce();
    }

    private void StartInitRoutine()
    {
        StopInitRoutine();
        _initCo = StartCoroutine(CoWaitAndInit());
    }

    private void StopInitRoutine()
    {
        if (_initCo != null)
        {
            StopCoroutine(_initCo);
            _initCo = null;
        }
    }

    private IEnumerator CoWaitAndInit()
    {
        float start = Time.unscaledTime;
        float lastWarn = -999f;

        while (true)
        {
            _s = AudioSettings.Instance;
            if (_s != null) break;

            float elapsed = Time.unscaledTime - start;

            if (warnAfterSeconds > 0f && elapsed >= warnAfterSeconds && Time.unscaledTime - lastWarn >= 0.5f)
            {
                lastWarn = Time.unscaledTime;
                Debug.LogError("[AudioSettingsUI] AudioSettings.Instance not found (waiting...)");
            }

            if (waitForInstanceTimeout > 0f && elapsed >= waitForInstanceTimeout)
            {
                Debug.LogError("[AudioSettingsUI] AudioSettings.Instance not found (timeout).");
                yield break;
            }

            yield return null;
        }

        SyncFromModel();
        RefreshButtonVisuals();
        BindOnce();
    }

    private void TryInitImmediate()
    {
        _s = AudioSettings.Instance;
        if (_s == null) return;

        SyncFromModel();
        RefreshButtonVisuals();
        BindOnce();
    }

    private void BindOnce()
    {
        if (_bound) return;
        _bound = true;

        if (muteAllButton) muteAllButton.onClick.AddListener(OnClickMuteAll);
        if (muteBgmButton) muteBgmButton.onClick.AddListener(OnClickMuteBgm);
        if (muteSfxButton) muteSfxButton.onClick.AddListener(OnClickMuteSfx); // FMOD SFX mute

        if (totalSlider) totalSlider.onValueChanged.AddListener(OnChangeMaster);
        if (bgmSlider) bgmSlider.onValueChanged.AddListener(OnChangeBgm);
        if (sfxSlider) sfxSlider.onValueChanged.AddListener(OnChangeSfx); // FMOD SFX volume
    }

    private void UnbindOnce()
    {
        if (!_bound) return;
        _bound = false;

        if (muteAllButton) muteAllButton.onClick.RemoveListener(OnClickMuteAll);
        if (muteBgmButton) muteBgmButton.onClick.RemoveListener(OnClickMuteBgm);
        if (muteSfxButton) muteSfxButton.onClick.RemoveListener(OnClickMuteSfx);

        if (totalSlider) totalSlider.onValueChanged.RemoveListener(OnChangeMaster);
        if (bgmSlider) bgmSlider.onValueChanged.RemoveListener(OnChangeBgm);
        if (sfxSlider) sfxSlider.onValueChanged.RemoveListener(OnChangeSfx);
    }

    private void SyncFromModel()
    {
        if (_s == null) return;

        totalSlider?.SetValueWithoutNotify(_s.MasterVol01);
        bgmSlider?.SetValueWithoutNotify(_s.BgmVol01);
        sfxSlider?.SetValueWithoutNotify(_s.SfxVol01);
    }

    private void AutoBind()
    {
        muteAllButton = muteAllButton ? muteAllButton : FindDeepComponent<Button>("MuteAll");
        muteBgmButton = muteBgmButton ? muteBgmButton : FindDeepComponent<Button>("MuteBGM");
        muteSfxButton = muteSfxButton ? muteSfxButton : FindDeepComponent<Button>("MuteSFX");

        bgmSlider = bgmSlider ? bgmSlider : FindDeepComponent<Slider>("BackgroundVS");
        sfxSlider = sfxSlider ? sfxSlider : FindDeepComponent<Slider>("SFXVS");
        totalSlider = totalSlider ? totalSlider : FindDeepComponent<Slider>("TotalVS");
    }

    private T FindDeepComponent<T>(string childName) where T : Component
    {
        var trs = GetComponentsInChildren<Transform>(true);
        foreach (var t in trs)
        {
            if (t.name == childName)
                return t.GetComponent<T>();
        }
        return null;
    }

    private void OnClickMuteAll()
    {
        if (_s == null) return;
        _s.ToggleMuteAll();
        RefreshButtonVisuals();
    }

    private void OnClickMuteBgm()
    {
        if (_s == null) return;
        _s.ToggleMuteBgm();
        RefreshButtonVisuals();
    }

    private void OnClickMuteSfx()
    {
        if (_s == null) return;
        _s.ToggleMuteSfx();
        RefreshButtonVisuals();
    }

    private void OnChangeMaster(float v)
    {
        if (_s == null) return;
        _s.SetMasterVolume01(v);

        if (autoUnmuteOnSliderChange && v > 0.001f && _s.MuteAll)
            _s.SetMuteAll(false);

        RefreshButtonVisuals();
    }

    private void OnChangeBgm(float v)
    {
        if (_s == null) return;
        _s.SetBgmVolume01(v);

        if (autoUnmuteOnSliderChange && v > 0.001f && _s.MuteBgm)
            _s.SetMuteBgm(false);

        RefreshButtonVisuals();
    }

    private void OnChangeSfx(float v)
    {
        if (_s == null) return;
        _s.SetSfxVolume01(v);

        if (autoUnmuteOnSliderChange && v > 0.001f && _s.MuteSfx)
            _s.SetMuteSfx(false);

        RefreshButtonVisuals();
    }

    private void RefreshButtonVisuals()
    {
        if (_s == null) return;

        SetButtonMutedVisual(muteAllButton, _s.MuteAll);
        SetButtonMutedVisual(muteBgmButton, _s.MuteBgm);
        SetButtonMutedVisual(muteSfxButton, _s.MuteSfx);
    }

    private void SetButtonMutedVisual(Button btn, bool muted)
    {
        if (!btn || !btn.image) return;
        var c = btn.image.color;
        c.a = muted ? mutedAlpha : 1f;
        btn.image.color = c;
    }
}