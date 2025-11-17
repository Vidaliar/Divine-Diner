using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveSystem
{
    bool HasSave(string profile, int slotIndex);
    SaveMeta GetMeta(string profile, int slotIndex);

    string GetThumbnailPath(string profile, int slotIndex);

    void SaveCurrentToSlot(string profile, int slotIndex, string title = null, string subtitle = null);
    void Load(string profile, int slotIndex);
    void Delete(string profile, int slotIndex);
}
