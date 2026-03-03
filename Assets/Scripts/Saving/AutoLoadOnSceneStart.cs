using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// On gameplay scene start, if GlobalLoadContext has a pending
/// request, call SaveSystem.Load(profile, slotIndex).
/// Attach this in gameplay scenes (NOT in the title scene).
/// </summary>
public class AutoLoadOnSceneStart : MonoBehaviour
{
    public SaveSystem saveSystem;

    private void Start()
    {
        if (!GlobalLoadContext.HasPendingRequest)
            return;

        if (saveSystem == null)
        {
            Debug.LogWarning("[AutoLoadOnSceneStart] SaveSystem reference is missing.");
            GlobalLoadContext.Clear();
            return;
        }

        Debug.Log($"[AutoLoadOnSceneStart] Loading from {GlobalLoadContext.ProfileName}/slot{GlobalLoadContext.SlotIndex}");
        saveSystem.Load(GlobalLoadContext.ProfileName, GlobalLoadContext.SlotIndex);
        GlobalLoadContext.Clear();
    }
}
