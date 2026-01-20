using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// StateProvider
/// ----------------------------------------------
/// Purpose:
///   Holds the current game progress and affection values
///   and knows how to capture/apply them into/from SaveData.
/// 
/// Usage:
///   1. Attach this script to some GameObject in the scene
///      (e.g., "GameState").
///   2. Assign this component to SaveSystem.stateProviderBehaviour
///      in the inspector.
///   3. Update currentDay/currentEpisode and affection values
///      from your story/episode logic.
///   4. SaveSystem will call Capture() / Apply() automatically
///      when saving or loading.
/// ----------------------------------------------
/// </summary>
public class StateProvider : MonoBehaviour, IStateProvider
{
    [Header("Story progress")]
    public int currentDay = 1;     // 1-7
    public int currentEpisode = 1; // 1-4

    [Header("Affection placeholders")]
    public int zeusAffinity;
    public int hermesAffinity;
    public int hephaestusAffinity;

    public SaveData Capture()
    {
        int clampedDay = Mathf.Clamp(currentDay, 1, 7);
        int clampedEpisode = Mathf.Clamp(currentEpisode, 1, 4);

        return new SaveData
        {
            day = clampedDay,
            episode = clampedEpisode,
            sceneName = SceneManager.GetActiveScene().name,

            zeusAffinity = zeusAffinity,
            hermesAffinity = hermesAffinity,
            hephaestusAffinity = hephaestusAffinity
        };
    }

    public IEnumerator Apply(SaveData data)
    {
        if (data == null)
            yield break;

        currentDay = Mathf.Clamp(data.day, 1, 7);
        currentEpisode = Mathf.Clamp(data.episode, 1, 4);

        zeusAffinity = data.zeusAffinity;
        hermesAffinity = data.hermesAffinity;
        hephaestusAffinity = data.hephaestusAffinity;

        yield return null;
    }
}
