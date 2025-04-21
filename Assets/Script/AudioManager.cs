using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private List<AudioAssetSO> _audioAssets = new List<AudioAssetSO>();

    public List<AudioAssetSO> AudioAssets => _audioAssets;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void PlayOneShot(EventReference soundEvent)
    {
        if (soundEvent.IsNull)
        {
            Debug.LogWarning("Sound event is null or invalid.");
            return;
        }
        RuntimeManager.PlayOneShot(soundEvent);
    }

    public static EventInstance CreateInstance(EventReference soundEvent)
    {
        if (soundEvent.IsNull)
        {
            Debug.LogWarning("Sound event is null or invalid.");
            return default;
        }
        return RuntimeManager.CreateInstance(soundEvent);
    }

    //Will be refactored in the future for neatness
    public void PlaySound(string soundName)
    {
        foreach (var audioAsset in _audioAssets)
        {
            foreach (var asset in audioAsset._audioAssets)
            {
                if (asset._name == soundName)
                {
                    PlayOneShot(asset._eventReference);
                    return;
                }
            }
        }
        Debug.LogWarning($"Sound '{soundName}' not found in AudioManager.");
    } 
}
