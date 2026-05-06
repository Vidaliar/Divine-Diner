using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StateProvider : MonoBehaviour, IStateProvider
{
    [Header("Story progress")]
    public int currentDay = 1;     // 1-7
    public int currentEpisode = 1; // 1-4

    [Header("Affection values")]
    public int zeus;         // 0-61
    public int hermes;       // 0-44
    public int hephaestus;   // 0-39

    [Header("Yarn dialogue state (node-level)")]
    public string currentYarnProject;
    public string currentYarnNode;
    public int currentYarnLineIndex;
    public string currentYarnLineTextID;

    public SaveData Capture()
    {
        int clampedDay = Mathf.Clamp(currentDay, 1, 7);
        int clampedEpisode = Mathf.Clamp(currentEpisode, 1, 4);

        return new SaveData
        {
            day = clampedDay,
            episode = clampedEpisode,
            sceneName = SceneManager.GetActiveScene().name,

            zeus = Mathf.Clamp(zeus, 0, 61),
            hermes = Mathf.Clamp(hermes, 0, 44),
            hephaestus = Mathf.Clamp(hephaestus, 0, 39),

            yarnProjectName = currentYarnProject,
            yarnNodeName = currentYarnNode,
            yarnLineIndex = currentYarnLineIndex,
            yarnLineTextID = currentYarnLineTextID
        };
    }

    public IEnumerator Apply(SaveData data)
    {
        if (data == null)
            yield break;

        currentDay = Mathf.Clamp(data.day, 1, 7);
        currentEpisode = Mathf.Clamp(data.episode, 1, 4);

        zeus = Mathf.Clamp(data.zeus, 0, 61);
        hermes = Mathf.Clamp(data.hermes, 0, 44);
        hephaestus = Mathf.Clamp(data.hephaestus, 0, 39);

        currentYarnProject = data.yarnProjectName;
        currentYarnNode = data.yarnNodeName;
        currentYarnLineIndex = data.yarnLineIndex;
        currentYarnLineTextID = data.yarnLineTextID;

        yield return null;
    }
    
    public void AddZeus(int amount)
    {
        zeus = Mathf.Clamp(zeus + amount, 0, 61);
        Debug.Log($"[StateProvider] Zeus affection changed by {amount}. Current = {zeus}");
    }

    public void AddHermes(int amount)
    {
        hermes = Mathf.Clamp(hermes + amount, 0, 44);
        Debug.Log($"[StateProvider] Hermes affection changed by {amount}. Current = {hermes}");
    }

    public void AddHephaestus(int amount)
    {
        hephaestus = Mathf.Clamp(hephaestus + amount, 0, 39);
        Debug.Log($"[StateProvider] Hephaestus affection changed by {amount}. Current = {hephaestus}");
    }

    public void SetAffectionValues(int zeusValue, int hermesValue, int hephaestusValue)
    {
        zeus = Mathf.Clamp(zeusValue, 0, 61);
        hermes = Mathf.Clamp(hermesValue, 0, 44);
        hephaestus = Mathf.Clamp(hephaestusValue, 0, 39);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        currentDay = Mathf.Clamp(currentDay, 1, 7);
        currentEpisode = Mathf.Clamp(currentEpisode, 1, 4);

        zeus = Mathf.Clamp(zeus, 0, 61);
        hermes = Mathf.Clamp(hermes, 0, 44);
        hephaestus = Mathf.Clamp(hephaestus, 0, 39);
    }
#endif
}