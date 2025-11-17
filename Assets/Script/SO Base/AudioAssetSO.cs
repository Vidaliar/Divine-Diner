using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

[CreateAssetMenu(fileName = "AudioAsset", menuName = "AudioAssetSO")]
public class AudioAssetSO : ScriptableObject
{
    public List<AudioAsset> _audioAssets = new List<AudioAsset>();
}

[System.Serializable]
public class AudioAsset{
    public string _name;
    public EventReference _eventReference;
}

