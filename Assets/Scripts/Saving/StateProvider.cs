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

        public int currentDay = 1;     // min 0
        public int currentEpisode = 1; // min 1

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
            currentDay = data.day;
            currentEpisode = data.episode;

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
