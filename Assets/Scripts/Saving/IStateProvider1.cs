using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface IStateProvider
{
    SaveData Capture();
    IEnumerator Apply(SaveData data);
}
