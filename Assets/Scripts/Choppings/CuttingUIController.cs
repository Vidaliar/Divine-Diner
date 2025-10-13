using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CuttingUIController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Progress")]
    [SerializeField] private Slider progressSlider;  // 0~1

    [Header("Space Text")]
    [SerializeField] private TMP_Text pressSpaceText;     // "Press Space"

    private void OnValidate()
    {
        if (root == null) root = gameObject;
    }

    public void Show(bool show)
    {
        if (root != null) root.SetActive(show);
    }

    /// <summary>
    /// currentCuts / totalCuts ¡ú Slider 0..1£»show¡°Press Space¡±while not completed
    /// </summary>
    public void UpdateProgress(int currentCuts, int totalCuts)
    {
        totalCuts = Mathf.Max(1, totalCuts);
        float v = Mathf.Clamp01((float)currentCuts / totalCuts);

        if (progressSlider != null) progressSlider.value = v;
        if (pressSpaceText != null) pressSpaceText.enabled = currentCuts < totalCuts;
    }
}
