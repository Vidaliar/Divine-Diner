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