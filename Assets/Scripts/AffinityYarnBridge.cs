using UnityEngine;
using Yarn.Unity;

/*
Usage Guide:
1. Attach this script to your persistent Manager object.
2. Assign Variable Storage with the same VariableStorageBehaviour used by your DialogueRunner.
3. StateProvider can be assigned manually, or this script will find it automatically at runtime.
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

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        ResolveReferences();

        if (syncOnStart)
        {
            SyncAllToYarn();
        }
    }

    private void ResolveReferences()
    {
        if (stateProvider == null)
        {
            stateProvider = FindObjectOfType<StateProvider>();

            if (stateProvider != null)
            {
                Debug.Log("[AffinityYarnBridge] Found StateProvider automatically: " + stateProvider.name);
            }
            else
            {
                Debug.LogWarning("[AffinityYarnBridge] Could not find StateProvider automatically.");
            }
        }

        if (variableStorage == null)
        {
            DialogueRunner runner = FindObjectOfType<DialogueRunner>();

            if (runner != null)
            {
                variableStorage = runner.VariableStorage;
                Debug.Log("[AffinityYarnBridge] Found VariableStorage automatically from DialogueRunner.");
            }
            else
            {
                Debug.LogWarning("[AffinityYarnBridge] Could not find DialogueRunner for VariableStorage.");
            }
        }
    }

    public void AddZeus(int amount)
    {
        ResolveReferences();

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
        ResolveReferences();

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
        ResolveReferences();

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
        ResolveReferences();

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