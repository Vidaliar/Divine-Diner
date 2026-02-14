using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface IStateProvider
{
    SaveData Capture();
    IEnumerator Apply(SaveData data);

    public class StateProvider : MonoBehaviour, IStateProvider
    {

        [Header("Story progress")]
        public int currentDay = 1;     // 1-7
        public int currentEpisode = 1; // 1-4

        [Header("Affection values")]
        public int zeus;
        public int hermes;
        public int hephaestus;

        [Header("Yarn dialogue state (node-level)")]
        [Tooltip("Last Yarn project name used when capturing save.")]
        public string currentYarnProject;

        [Tooltip("Last Yarn node name seen when capturing save.")]
        public string currentYarnNode;

        [Tooltip("Reserved for future line-level resume. Currently unused.")]
        public int currentYarnLineIndex;

        public SaveData Capture()
        {
            int clampedDay = Mathf.Clamp(currentDay, 1, 7);
            int clampedEpisode = Mathf.Clamp(currentEpisode, 1, 4);

            return new SaveData
            {
                day = clampedDay,
                episode = clampedEpisode,
                sceneName = SceneManager.GetActiveScene().name,

                zeus = zeus,
                hermes = hermes,
                hephaestus = hephaestus,

                yarnProjectName = currentYarnProject,
                yarnNodeName = currentYarnNode,
                yarnLineIndex = currentYarnLineIndex
            };
        }

        public IEnumerator Apply(SaveData data)
        {
            if (data == null)
                yield break;

            currentDay = Mathf.Clamp(data.day, 1, 7);
            currentEpisode = Mathf.Clamp(data.episode, 1, 4);

            zeus = data.zeus;
            hermes = data.hermes;
            hephaestus = data.hephaestus;

            currentYarnProject = data.yarnProjectName;
            currentYarnNode = data.yarnNodeName;
            currentYarnLineIndex = data.yarnLineIndex;

            yield return null;
        }
    }
}
