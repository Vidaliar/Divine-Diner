using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStateProvider
{
    SaveData Capture();
    IEnumerator Apply(SaveData data);

    public class StateProvider : MonoBehaviour, IStateProvider
    {

        public int currentDay = 1;// min 0
        public SaveData Capture()
        {
            return new SaveData
            {
                day = currentDay,
                // ====== future: more data to save ======
                // scriptId = myStory.CurrentScriptId;
                // label    = myStory.CurrentLabel;
                // lineIndex= myStory.CurrentLineIndex;
                // bgmId = myAudio.CurrentBgmId; bgmTime = myAudio.CurrentTime;
                // flags = myFlow.GetFlags();
            };
        }
        public IEnumerator Apply(SaveData data)
        {
            if (data == null) yield break;

            // restore day
            currentDay = data.day;

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
