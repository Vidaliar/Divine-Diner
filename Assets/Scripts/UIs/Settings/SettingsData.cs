using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
================================================================================
SettingsData.cs — Plain data container for game settings
================================================================================

IMPLEMENTATION GUIDE (How to use)

1) Purpose
   - This class is a simple serializable data container.
   - It holds all user-facing settings (volume, graphics, language, etc.).
   - It is used by:
     * SettingsManager: as the "current" active settings.
     * SettingsMenuUI: as "editing" data and "snapshot" data.

2) Usage
   - Do NOT attach this script to any GameObject.
   - Create and use it through code only:
       var data = new SettingsData();
       data.masterVolume = 0.5f;

   - To avoid reference sharing issues, always use Clone() when you need
     a separate copy:
       var snapshot = data.Clone();
       var editing  = data.Clone();

3) Extending
   - When you introduce new settings, add public fields here, e.g.:
       public int resolutionIndex;
       public string languageCode;
       public float textSpeed;
   - Then update SettingsMenuUI to bind new fields to UI controls.
   - Finally, update SettingsManager.ApplyCurrent() to apply them to the game.

NOTES
- Fields here are placeholders and can be safely replaced/extended later.
- Keep this class as a "dumb" data holder (no scene references, no MonoBehaviour).
================================================================================
*/

public class SettingsData
{
    // ===== Placeholder settings fields =====
    // Replace or extend these as needed.

    // Master volume of the game (0.0 ~ 1.0)
    public float masterVolume = 1f;

    // BGM (background music) volume (0.0 ~ 1.0)
    public float bgmVolume = 1f;

    // Whether the game is running in full screen mode
    public bool isFullScreen = true;

    // Create a deep copy so snapshots and editing data do not share references
    public SettingsData Clone()
    {
        return new SettingsData
        {
            masterVolume = this.masterVolume,
            bgmVolume = this.bgmVolume,
            isFullScreen = this.isFullScreen
        };
    }
}