using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SaveSystem saveSystem;

    [Header("Profile")]
    [SerializeField] private string manualProfile = "Default";

    public void SaveToSlot(int slotIndex)
    {
        if (saveSystem == null)
        {
            Debug.LogWarning("[SaveLoadMenuController] SaveSystem is missing.");
            return;
        }

        saveSystem.SaveCurrentToSlot(manualProfile, slotIndex);
    }

    public void LoadFromSlot(int slotIndex)
    {
        if (saveSystem == null)
        {
            Debug.LogWarning("[SaveLoadMenuController] SaveSystem is missing.");
            return;
        }

        saveSystem.Load(manualProfile, slotIndex);
    }

    public void DeleteSlot(int slotIndex)
    {
        if (saveSystem == null)
        {
            Debug.LogWarning("[SaveLoadMenuController] SaveSystem is missing.");
            return;
        }

        saveSystem.Delete(manualProfile, slotIndex);
    }

    public void SaveToAutoSlot()
    {
        if (saveSystem == null)
        {
            Debug.LogWarning("[SaveLoadMenuController] SaveSystem is missing.");
            return;
        }

        saveSystem.SaveCurrentToSlot("AutoSave", 0);
    }

    public void LoadAutoSlot()
    {
        if (saveSystem == null)
        {
            Debug.LogWarning("[SaveLoadMenuController] SaveSystem is missing.");
            return;
        }

        saveSystem.Load("AutoSave", 0);
    }
}
