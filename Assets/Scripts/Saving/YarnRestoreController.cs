using System.Collections;
using UnityEngine;
using Yarn.Unity;

[RequireComponent(typeof(DialogueRunner))]
public class YarnRestoreController : MonoBehaviour
{
    public static bool IsRestoring { get; private set; }

    [Header("References")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private StateProvider stateProvider;

    [Header("Restore Settings")]
    [SerializeField] private bool disableStartAutomaticallyWhenPendingLoad = true;
    [SerializeField] private int safetyFrameLimit = 2000;

    private void Awake()
    {
        if (dialogueRunner == null)
            dialogueRunner = GetComponent<DialogueRunner>();

        if (stateProvider == null)
            stateProvider = FindObjectOfType<StateProvider>();

        if (disableStartAutomaticallyWhenPendingLoad &&
            dialogueRunner != null &&
            GlobalLoadContext.HasPendingRequest)
        {
            dialogueRunner.startAutomatically = false;
        }
    }

    public void RestoreFromSavedState()
    {
        StartCoroutine(RestoreRoutine());
    }

    private IEnumerator RestoreRoutine()
{
    if (dialogueRunner == null)
        dialogueRunner = GetComponent<DialogueRunner>();

    if (stateProvider == null)
        stateProvider = FindObjectOfType<StateProvider>();

    if (dialogueRunner == null)
    {
        Debug.LogWarning("[YarnRestoreController] DialogueRunner reference is missing.");
        yield break;
    }

    if (stateProvider == null)
    {
        Debug.LogWarning("[YarnRestoreController] StateProvider reference is missing.");
        yield break;
    }

    // Cache saved target first
    string targetNode = stateProvider.currentYarnNode;
    int targetLineIndex = stateProvider.currentYarnLineIndex;
    string targetTextID = stateProvider.currentYarnLineTextID;

    if (string.IsNullOrEmpty(targetNode))
    {
        Debug.Log("[YarnRestoreController] No saved Yarn node. Skip restore.");
        yield break;
    }

    Debug.Log($"[YarnRestoreController] Begin restore -> node={targetNode}, lineIndex={targetLineIndex}, textID={targetTextID}");

    if (dialogueRunner.IsDialogueRunning)
    {
        dialogueRunner.Stop();
        yield return null;
    }

    IsRestoring = true;

    // IMPORTANT:
    // Clear the live tracker values so we don't immediately think
    // we've already reached the target before dialogue actually restarts.
    stateProvider.currentYarnNode = string.Empty;
    stateProvider.currentYarnLineIndex = -999;
    stateProvider.currentYarnLineTextID = string.Empty;

    dialogueRunner.StartDialogue(targetNode);

    int safety = 0;

    // Wait until dialogue is running
    while (!dialogueRunner.IsDialogueRunning && safety < safetyFrameLimit)
    {
        safety++;
        yield return null;
    }

    safety = 0;

    // Wait until tracker has seen the first line of the restarted node
    while (!HasStartedTrackingTargetNode(stateProvider, targetNode) && safety < safetyFrameLimit)
    {
        safety++;
        yield return null;
    }

    // If saved at the beginning of a node, stop here
    if (targetLineIndex <= 0 && string.IsNullOrEmpty(targetTextID))
    {
        IsRestoring = false;
        Debug.Log($"[YarnRestoreController] Restore complete at node start -> node={stateProvider.currentYarnNode}, lineIndex={stateProvider.currentYarnLineIndex}, textID={stateProvider.currentYarnLineTextID}");
        yield break;
    }

    safety = 0;

    // Fast-forward until we reach the saved line
    while (!HasReachedTargetAfterRestart(stateProvider, targetLineIndex, targetTextID) &&
           safety < safetyFrameLimit)
    {
        AdvanceAllViews();
        safety++;
        yield return null;
    }

    IsRestoring = false;

    if (safety >= safetyFrameLimit)
    {
        Debug.LogWarning("[YarnRestoreController] Restore stopped by safety limit.");
    }
    else
    {
        Debug.Log($"[YarnRestoreController] Restore complete -> node={stateProvider.currentYarnNode}, lineIndex={stateProvider.currentYarnLineIndex}, textID={stateProvider.currentYarnLineTextID}");
    }
}

private bool HasStartedTrackingTargetNode(StateProvider provider, string targetNode)
{
    if (provider == null)
        return false;

    return provider.currentYarnNode == targetNode && provider.currentYarnLineIndex >= 0;
}

private bool HasReachedTargetAfterRestart(StateProvider provider, int targetLineIndex, string targetTextID)
{
    if (provider == null)
        return false;

    if (!string.IsNullOrEmpty(targetTextID))
    {
        return provider.currentYarnLineTextID == targetTextID;
    }

    return provider.currentYarnLineIndex >= targetLineIndex;
}

    private bool HasReachedTarget(StateProvider provider, int targetLineIndex, string targetTextID)
    {
        if (provider == null)
            return false;

        if (!string.IsNullOrEmpty(targetTextID))
        {
            return provider.currentYarnLineTextID == targetTextID;
        }

        return provider.currentYarnLineIndex >= targetLineIndex;
    }

    private void AdvanceAllViews()
    {
        if (dialogueRunner == null || dialogueRunner.dialogueViews == null)
            return;

        foreach (var view in dialogueRunner.dialogueViews)
        {
            if (view == null || !view.isActiveAndEnabled)
                continue;

            view.UserRequestedViewAdvancement();
        }
    }
}