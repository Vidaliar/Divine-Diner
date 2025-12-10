using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SaveMeta
{
    public string title;         // Like "Day 2"
    public string subtitle;      // like "save time: 9/20/2028 6:17 am"
    public string thumbnailPath; // PNG path
    public string timeISO;       // Save time in ISO format
}

[Serializable]
public class SaveData
{
    public int day;
    public int episode;
    public string sceneName;

    // ====== Future possible data to save ======
    // public string scriptId;
    // public string label;
    // public int lineIndex;
    // public string bgmId;
    // public float bgmTime;
    // public string[] flags;
    // public float volumeBgm;
    // public float volumeSe;
}

[Serializable]
public class SaveFile
{
    public SaveMeta meta;
    public SaveData data;
}