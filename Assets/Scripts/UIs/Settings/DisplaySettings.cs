using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Usage Guide:
/// 1. Create two TMP_Dropdowns in your Settings UI:
///    - Resolution dropdown
///    - Window mode dropdown
///
/// 2. Create an Apply button.
///
/// 3. Attach this script to a Settings manager object or any active UI object.
///
/// 4. Drag the two TMP_Dropdowns and the Apply button into the Inspector fields.
///
/// 5. The dropdowns only change pending values.
///    The actual screen settings are applied only when the Apply button is pressed.
///
/// If there is no saved window mode, the default dropdown value is Fullscreen.
/// </summary>
public class DisplaySettings : MonoBehaviour
{
    [Serializable]
    public class ResolutionOption
    {
        public string label;
        public int width;
        public int height;

        public ResolutionOption(string label, int width, int height)
        {
            this.label = label;
            this.width = width;
            this.height = height;
        }
    }

    [Header("UI References")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown windowModeDropdown;
    [SerializeField] private Button applyButton;

    [Header("Resolution Options")]
    [SerializeField] private ResolutionOption[] resolutionOptions =
    {
        new ResolutionOption("FHD 1920 x 1080", 1920, 1080),
        new ResolutionOption("2K 2560 x 1440", 2560, 1440)
    };

    private readonly FullScreenMode[] windowModes =
    {
        FullScreenMode.Windowed,
        FullScreenMode.FullScreenWindow,
        FullScreenMode.ExclusiveFullScreen
    };

    private readonly string[] windowModeLabels =
    {
        "Windowed",
        "Borderless",
        "Fullscreen"
    };

    private const int DefaultWindowModeIndex = 2;

    private const string ResolutionIndexKey = "Display_ResolutionIndex";
    private const string WindowModeIndexKey = "Display_WindowModeIndex";

    private void Awake()
    {
        SetupResolutionDropdown();
        SetupWindowModeDropdown();
        LoadSavedSettingsToDropdowns();

        if (applyButton != null)
        {
            applyButton.onClick.RemoveListener(ApplySelectedSettings);
            applyButton.onClick.AddListener(ApplySelectedSettings);
        }
    }

    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();

        List<string> labels = new List<string>();

        foreach (ResolutionOption option in resolutionOptions)
        {
            labels.Add(option.label);
        }

        resolutionDropdown.AddOptions(labels);
    }

    private void SetupWindowModeDropdown()
    {
        if (windowModeDropdown == null) return;

        windowModeDropdown.ClearOptions();
        windowModeDropdown.AddOptions(new List<string>(windowModeLabels));
    }

    private void LoadSavedSettingsToDropdowns()
    {
        int resolutionIndex = PlayerPrefs.GetInt(
            ResolutionIndexKey,
            FindClosestResolutionIndex(Screen.width, Screen.height)
        );

        int windowModeIndex = PlayerPrefs.GetInt(
            WindowModeIndexKey,
            DefaultWindowModeIndex
        );

        resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutionOptions.Length - 1);
        windowModeIndex = Mathf.Clamp(windowModeIndex, 0, windowModes.Length - 1);

        if (resolutionDropdown != null)
        {
            resolutionDropdown.SetValueWithoutNotify(resolutionIndex);
        }

        if (windowModeDropdown != null)
        {
            windowModeDropdown.SetValueWithoutNotify(windowModeIndex);
        }
    }

    public void ApplySelectedSettings()
    {
        if (resolutionDropdown == null || windowModeDropdown == null)
        {
            Debug.LogWarning("[DisplaySettings] Missing dropdown reference.");
            return;
        }

        int resolutionIndex = Mathf.Clamp(
            resolutionDropdown.value,
            0,
            resolutionOptions.Length - 1
        );

        int windowModeIndex = Mathf.Clamp(
            windowModeDropdown.value,
            0,
            windowModes.Length - 1
        );

        ResolutionOption selectedResolution = resolutionOptions[resolutionIndex];
        FullScreenMode selectedMode = windowModes[windowModeIndex];

        Screen.SetResolution(
            selectedResolution.width,
            selectedResolution.height,
            selectedMode
        );

        PlayerPrefs.SetInt(ResolutionIndexKey, resolutionIndex);
        PlayerPrefs.SetInt(WindowModeIndexKey, windowModeIndex);
        PlayerPrefs.Save();

        Debug.Log(
            $"[DisplaySettings] Applied: " +
            $"{selectedResolution.width}x{selectedResolution.height}, {selectedMode}"
        );
    }

    private int FindClosestResolutionIndex(int currentWidth, int currentHeight)
    {
        int bestIndex = 0;
        int bestDifference = int.MaxValue;

        for (int i = 0; i < resolutionOptions.Length; i++)
        {
            int widthDifference = Mathf.Abs(resolutionOptions[i].width - currentWidth);
            int heightDifference = Mathf.Abs(resolutionOptions[i].height - currentHeight);
            int totalDifference = widthDifference + heightDifference;

            if (totalDifference < bestDifference)
            {
                bestDifference = totalDifference;
                bestIndex = i;
            }
        }

        return bestIndex;
    }
}