using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using static IStateProvider;



/// <summary>
/// YarnBeatTracer
/// ----------------------------------------------
/// Purpose:
///   Bridges Yarn's DialogueRunner and your Save/Load system.
///
///   - Tracks which Yarn node is currently running and writes
///     that info into StateProvider (currentYarnProject/currentYarnNode),
///     so it is serialized into SaveData when saving.
///   - On scene start (after a load), optionally resumes Yarn at
///     the saved node. If there is no saved node, can start from
///     a default node.
///
/// Usage:
///   1. Put this on a GameObject in each scene that has a DialogueRunner.
///   2. In inspector:
///        - Runner: assign your DialogueRunner, or leave empty to auto-find.
///        - State Provider: assign the StateProvider, or leave empty to auto-find.
///        - Resume From Save On Start: usually true in gameplay scenes.
///        - Default Start Node: name of the node to use when there is no
///          saved node (e.g. for New Game).
///   3. SaveSystem will call StateProvider.Capture()/Apply() as usual;
///      this tracer just keeps Yarn-related fields in StateProvider up to date.
/// ----------------------------------------------
/// </summary>
public class YarnBeatTracer : MonoBehaviour
{
    [Tooltip("If left empty, will auto-find the DialogueRunner in the scene.")]
    public DialogueRunner runner;

    [Tooltip("If left empty, will auto-find the StateProvider in the scene.")]
    public StateProvider stateProvider;

    [Header("Behaviour")]
    [Tooltip("If true, tries to resume Yarn from the saved node on scene start.")]
    public bool resumeFromSaveOnStart = true;

    [Tooltip("Fallback node to start when there is no saved node (e.g. new game).")]
    public string defaultStartNode;


    void Awake()
    {
        if (runner == null) runner = FindObjectOfType<DialogueRunner>();
        if (stateProvider == null)
            stateProvider = FindObjectOfType<StateProvider>();
        if (runner == null)
        {
            Debug.LogWarning("[YARN] No DialogueRunner found in this scene.");
            enabled = false;
            return;
        }

        // Basic wiring info
        Debug.Log($"[YARN] Runner: {runner.name} | StartAutomatically={runner.startAutomatically} | StartNode=\"{runner.startNode}\"");

        // Yarn Project name (so we know which .yarnproject to open)
        if (runner.yarnProject != null)
        {
            Debug.Log($"[YARN] Project: {runner.yarnProject.name}");
        }
        else
        {
            Debug.LogWarning("[YARN] Runner has no Yarn Project assigned.");
        }

        // Node tracing
        runner.onNodeStart.AddListener(OnNodeStart);
        runner.onNodeComplete.AddListener(OnNodeComplete);
        runner.onDialogueComplete.AddListener(OnDialogueComplete);
    }

    private void OnDestroy()
    {
        if (runner == null)
            return;

        runner.onNodeStart.RemoveListener(OnNodeStart);
        runner.onNodeComplete.RemoveListener(OnNodeComplete);
        runner.onDialogueComplete.RemoveListener(OnDialogueComplete);
    }

    private void Start()
    {
        if (!resumeFromSaveOnStart)
            return;

        // Let SaveSystem.Load + StateProvider.Apply finish first
        StartCoroutine(ResumeFromSavedNodeCoroutine());
    }

    private IEnumerator ResumeFromSavedNodeCoroutine()
    {
        yield return null;
        yield return null;

        if (runner == null)
            yield break;

        string nodeToStart = null;

        if (stateProvider != null && !string.IsNullOrEmpty(stateProvider.currentYarnNode))
        {
            nodeToStart = stateProvider.currentYarnNode;
            Debug.Log($"[YarnBeatTracer] Resuming Yarn node from save: {nodeToStart}");
        }
        else if (!string.IsNullOrEmpty(defaultStartNode))
        {
            nodeToStart = defaultStartNode;
            Debug.Log($"[YarnBeatTracer] Starting default Yarn node: {nodeToStart}");
        }

        if (!string.IsNullOrEmpty(nodeToStart))
        {
            runner.StartDialogue(nodeToStart);
        }
    }

    private void OnNodeStart(string nodeName)
    {
        Debug.Log($"[YarnBeatTracer] Start node: {nodeName}");

        if (stateProvider != null)
        {
            stateProvider.currentYarnNode = nodeName;

            if (runner != null && runner.yarnProject != null)
            {
                stateProvider.currentYarnProject = runner.yarnProject.name;
            }

            // line-level resume not implemented yet
            stateProvider.currentYarnLineIndex = 0;
        }
    }

    private void OnNodeComplete(string nodeName)
    {
        Debug.Log($"[YarnBeatTracer] End node: {nodeName}");
    }

    private void OnDialogueComplete()
    {
        Debug.Log("[YarnBeatTracer] Dialogue complete");
    }
}

