using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;



public class YarnBeatTracer : MonoBehaviour
{
    [Tooltip("If left empty, will auto-find the DialogueRunner in the scene.")]
    public DialogueRunner runner;

    void Awake()
    {
        if (runner == null) runner = FindObjectOfType<DialogueRunner>();
        if (runner == null)
        {
            Debug.LogWarning("[YARN] No DialogueRunner found in this scene.");
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
        runner.onNodeStart.AddListener(nodeName => Debug.Log($"[YARN] Start node: {nodeName}"));
        runner.onNodeComplete.AddListener(nodeName => Debug.Log($"[YARN] End node: {nodeName}"));
        runner.onDialogueComplete.AddListener(() => Debug.Log("[YARN] Dialogue complete"));
    }
}

