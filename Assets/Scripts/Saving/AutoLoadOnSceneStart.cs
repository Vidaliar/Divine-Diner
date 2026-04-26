using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoLoadOnSceneStart : MonoBehaviour
{
    [SerializeField] private SaveSystem saveSystem;

    private void Awake()
    {
        if (saveSystem == null)
        {
            saveSystem = GetComponent<SaveSystem>();

            if (saveSystem == null)
            {
                saveSystem = FindFirstObjectByType<SaveSystem>();

                if (saveSystem == null)
                {
                    saveSystem = FindObjectOfType<SaveSystem>();
                }
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!GlobalLoadContext.HasPendingRequest)
            return;

        StartCoroutine(LoadAfterSceneReady());
    }

    private IEnumerator LoadAfterSceneReady()
    {
        // Wait one frame so scene objects finish Awake/OnEnable/Start order more safely
        yield return null;

        if (!GlobalLoadContext.HasPendingRequest)
            yield break;

        if (saveSystem == null)
        {
            saveSystem = GetComponent<SaveSystem>();

            if (saveSystem == null)
            {
                saveSystem = FindFirstObjectByType<SaveSystem>();

                if (saveSystem == null)
                {
                    saveSystem = FindObjectOfType<SaveSystem>();
                }
            }
        }

        if (saveSystem == null)
        {
            Debug.LogWarning("[AutoLoadOnSceneStart] SaveSystem reference is missing.");
            GlobalLoadContext.Clear();
            yield break;
        }

        Debug.Log($"[AutoLoadOnSceneStart] Loading from {GlobalLoadContext.ProfileName}/slot{GlobalLoadContext.SlotIndex}");
        saveSystem.Load(GlobalLoadContext.ProfileName, GlobalLoadContext.SlotIndex);
        GlobalLoadContext.Clear();
    }
}