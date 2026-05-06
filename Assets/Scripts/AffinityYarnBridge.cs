using UnityEngine;
using Yarn.Unity;

/*
Usage Guide:
1. Attach this script to your persistent Manager object.
2. Assign Variable Storage with the same VariableStorageBehaviour used by your DialogueRunner.
3. Assign your existing StateProvider.
4. Call SyncAllToYarn() after loading save data, after changing affection, and before starting dialogue.
5. Yarn variables created by this script:
   $zeusAffinity
   $hermesAffinity
   $hephaestusAffinity
*/

public class AffinityYarnBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VariableStorageBehaviour variableStorage;
    [SerializeField] private StateProvider stateProvider;

    [Header("Startup")]
    [SerializeField] private bool syncOnStart = true;

    private void Start()
    {
        if (syncOnStart)
        {
            SyncAllToYarn();
        }
    }

    public void AddZeus(int amount)
    {
        if (stateProvider == null)
        {
            Debug.LogWarning("[AffinityYarnBridge] StateProvider is missing.");
            return;
        }

        stateProvider.AddZeus(amount);
        SyncAllToYarn();
    }

    public void AddHermes(int amount)
    {
        if (stateProvider == null)
        {
            Debug.LogWarning("[AffinityYarnBridge] StateProvider is missing.");
            return;
        }

        stateProvider.AddHermes(amount);
        SyncAllToYarn();
    }

    public void AddHephaestus(int amount)
    {
        if (stateProvider == null)
        {
            Debug.LogWarning("[AffinityYarnBridge] StateProvider is missing.");
            return;
        }

        stateProvider.AddHephaestus(amount);
        SyncAllToYarn();
    }

    public void SyncAllToYarn()
    {
        if (variableStorage == null)
        {
            Debug.LogWarning("[AffinityYarnBridge] VariableStorage is missing.");
            return;
        }

        if (stateProvider == null)
        {
            Debug.LogWarning("[AffinityYarnBridge] StateProvider is missing.");
            return;
        }

        variableStorage.SetValue("$zeusAffinity", (float)stateProvider.zeus);
        variableStorage.SetValue("$hermesAffinity", (float)stateProvider.hermes);
        variableStorage.SetValue("$hephaestusAffinity", (float)stateProvider.hephaestus);

        Debug.Log(
            $"[AffinityYarnBridge] Synced affection to Yarn. " +
            $"Zeus={stateProvider.zeus}, " +
            $"Hermes={stateProvider.hermes}, " +
            $"Hephaestus={stateProvider.hephaestus}"
        );
    }
}