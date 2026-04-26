using System;
using UnityEngine;
using Yarn.Unity;

[RequireComponent(typeof(DialogueRunner))]
public class YarnSaveTrackerView : DialogueViewBase
{
    [Header("References")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private StateProvider stateProvider;

    [Header("Optional")]
    [Tooltip("If each day uses a different Yarn project name and you want to save it, fill it here. Otherwise you can leave it empty.")]
    [SerializeField] private string yarnProjectNameOverride = "";

    private int currentLineIndex = -1;
    private string currentNodeName = "";

    private void Awake()
    {
        if (dialogueRunner == null)
            dialogueRunner = GetComponent<DialogueRunner>();

        ResolveStateProvider();
    }

    private void OnEnable()
    {
        ResolveStateProvider();

        if (dialogueRunner == null)
            dialogueRunner = GetComponent<DialogueRunner>();

        if (dialogueRunner != null)
        {
            dialogueRunner.onNodeStart.AddListener(HandleNodeStart);
        }
    }

    private void OnDisable()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.onNodeStart.RemoveListener(HandleNodeStart);
        }
    }

    private void ResolveStateProvider()
    {
        if (stateProvider == null)
        {
            stateProvider = FindFirstObjectByType<StateProvider>();

            if (stateProvider == null)
            {
                stateProvider = FindObjectOfType<StateProvider>();
            }
        }
    }

    private void HandleNodeStart(string nodeName)
    {
        ResolveStateProvider();

        currentNodeName = nodeName;
        currentLineIndex = -1;

        if (stateProvider == null)
            return;

        if (!string.IsNullOrEmpty(yarnProjectNameOverride))
            stateProvider.currentYarnProject = yarnProjectNameOverride;

        stateProvider.currentYarnNode = currentNodeName;
        stateProvider.currentYarnLineIndex = -1;
        stateProvider.currentYarnLineTextID = string.Empty;
    }

    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
    {
        ResolveStateProvider();

        if (dialogueRunner != null && !string.IsNullOrEmpty(dialogueRunner.CurrentNodeName))
        {
            if (currentNodeName != dialogueRunner.CurrentNodeName)
            {
                HandleNodeStart(dialogueRunner.CurrentNodeName);
            }
        }

        currentLineIndex++;

        if (stateProvider != null)
        {
            if (!string.IsNullOrEmpty(yarnProjectNameOverride))
                stateProvider.currentYarnProject = yarnProjectNameOverride;

            stateProvider.currentYarnNode = currentNodeName;
            stateProvider.currentYarnLineIndex = currentLineIndex;
            stateProvider.currentYarnLineTextID = dialogueLine.TextID;
        }

        onDialogueLineFinished?.Invoke();
    }
}