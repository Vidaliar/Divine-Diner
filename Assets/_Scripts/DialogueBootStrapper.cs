using UnityEngine;
using Yarn.Unity;

public class DialogueBootstrapper : MonoBehaviour
{
    public DialogueRunner runner;
    public string defaultStartNode = "Start";  // Zeus

    void Awake()
    {
        if (!runner) runner = FindObjectOfType<DialogueRunner>();
        if (runner) runner.startAutomatically = false;   // hard-disable autostart
    }

    void Start()
    {
        if (!runner) return;
        var nodeToStart = string.IsNullOrEmpty(VNReturn.NextNode)
            ? defaultStartNode
            : VNReturn.NextNode;

        VNReturn.NextNode = null;               // consume once
        runner.StartDialogue(nodeToStart);      // Start OR Hermes_test_* as set
    }
}

