using System.Collections;

public interface IStateProvider
{
    SaveData Capture();
    IEnumerator Apply(SaveData data);
}
