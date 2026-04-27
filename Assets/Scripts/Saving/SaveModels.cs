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
    public int day;          // 1-7
    public int episode;      // 1-4
    public string sceneName;

    public int zeus;
    public int hermes;
    public int hephaestus;

    public string yarnProjectName;
    public string yarnNodeName;
    public int yarnLineIndex;
    public string yarnLineTextID;

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