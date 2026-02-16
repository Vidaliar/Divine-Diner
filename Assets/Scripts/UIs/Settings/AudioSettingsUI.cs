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

    private AudioSettings S => AudioSettings.Instance;

    private void Awake()
    {
        if (autoBindByName) AutoBind();
    }

    private void OnEnable()
    {
        if (S == null)
        {
            Debug.LogError("[AudioSettingsUI] AudioSettings.Instance not found");
            return;
        }

        totalSlider?.SetValueWithoutNotify(S.MasterVol01);
        bgmSlider?.SetValueWithoutNotify(S.BgmVol01);
        sfxSlider?.SetValueWithoutNotify(S.SfxVol01);

        RefreshButtonVisuals();

        if (muteAllButton) muteAllButton.onClick.AddListener(OnClickMuteAll);
        if (muteBgmButton) muteBgmButton.onClick.AddListener(OnClickMuteBgm);
        if (muteSfxButton) muteSfxButton.onClick.AddListener(OnClickMuteSfx); // placeholder

        if (totalSlider) totalSlider.onValueChanged.AddListener(OnChangeMaster);
        if (bgmSlider) bgmSlider.onValueChanged.AddListener(OnChangeBgm);
        if (sfxSlider) sfxSlider.onValueChanged.AddListener(OnChangeSfx); // placeholder
    }

    private void OnDisable()
    {
        if (muteAllButton) muteAllButton.onClick.RemoveListener(OnClickMuteAll);
        if (muteBgmButton) muteBgmButton.onClick.RemoveListener(OnClickMuteBgm);
        if (muteSfxButton) muteSfxButton.onClick.RemoveListener(OnClickMuteSfx);

        if (totalSlider) totalSlider.onValueChanged.RemoveListener(OnChangeMaster);
        if (bgmSlider) bgmSlider.onValueChanged.RemoveListener(OnChangeBgm);
        if (sfxSlider) sfxSlider.onValueChanged.RemoveListener(OnChangeSfx);
    }

    private void AutoBind()
    {
        muteAllButton = muteAllButton ? muteAllButton : transform.Find("MuteAll")?.GetComponent<Button>();
        muteBgmButton = muteBgmButton ? muteBgmButton : transform.Find("MuteBGM")?.GetComponent<Button>();
        muteSfxButton = muteSfxButton ? muteSfxButton : transform.Find("MuteSFX")?.GetComponent<Button>();

        bgmSlider = bgmSlider ? bgmSlider : transform.Find("BackgroundVS")?.GetComponent<Slider>();
        sfxSlider = sfxSlider ? sfxSlider : transform.Find("SFXVS")?.GetComponent<Slider>();
        totalSlider = totalSlider ? totalSlider : transform.Find("TotalVS")?.GetComponent<Slider>();
    }

    private void OnClickMuteAll()
    {
        S.ToggleMuteAll();
        RefreshButtonVisuals();
    }

    private void OnClickMuteBgm()
    {
        S.ToggleMuteBgm();
        RefreshButtonVisuals();
    }

    private void OnClickMuteSfx()
    {
        S.ToggleMuteSfx(); // placeholder
        RefreshButtonVisuals();
    }

    private void OnChangeMaster(float v)
    {
        S.SetMasterVolume01(v);
        if (autoUnmuteOnSliderChange && v > 0.001f && S.MuteAll) S.SetMuteAll(false);
        RefreshButtonVisuals();
    }

    private void OnChangeBgm(float v)
    {
        S.SetBgmVolume01(v);
        if (autoUnmuteOnSliderChange && v > 0.001f && S.MuteBgm) S.SetMuteBgm(false);
        RefreshButtonVisuals();
    }

    private void OnChangeSfx(float v)
    {
        S.SetSfxVolume01(v); // placeholder
        if (autoUnmuteOnSliderChange && v > 0.001f && S.MuteSfx) S.SetMuteSfx(false);
        RefreshButtonVisuals();
    }

    private void RefreshButtonVisuals()
    {
        SetButtonMutedVisual(muteAllButton, S.MuteAll);
        SetButtonMutedVisual(muteBgmButton, S.MuteBgm);
        SetButtonMutedVisual(muteSfxButton, S.MuteSfx);
    }

    private void SetButtonMutedVisual(Button btn, bool muted)
    {
        if (!btn || !btn.image) return;
        var c = btn.image.color;
        c.a = muted ? mutedAlpha : 1f;
        btn.image.color = c;
    }
}
