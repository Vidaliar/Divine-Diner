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

        public int currentDay = 1;     // 1-7
        public int currentEpisode = 1; // 1-4
        public int zeus;
        public int hermes;
        public int hephaestus;

        public SaveData Capture()
        {
            return new SaveData
            {
                day = currentDay,
                episode = currentEpisode,
                sceneName = SceneManager.GetActiveScene().name,
            };
        }

        public IEnumerator Apply(SaveData data)
        {
            if (data == null)
                yield break;

            // restore day & episode
            currentDay = Mathf.Clamp(data.day, 1, 7);
            currentEpisode = Mathf.Clamp(data.episode, 1, 4);

            zeus = data.zeus;
            hermes = data.hermes;
            hephaestus = data.hephaestus;

            // ====== future: drive the story to the right position ======
            // myStory.LoadScript(data.scriptId);
            // yield return myStory.JumpToLabelAsync(data.label);
            // myStory.SetLineIndex(data.lineIndex);
            // myAudio.PlayBGM(data.bgmId, atTime: data.bgmTime);
            // myFlow.SetFlags(data.flags);

            yield return null;
        }
    }
}
