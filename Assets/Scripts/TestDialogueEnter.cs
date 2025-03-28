using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDialogueEnter : MonoBehaviour
{
    [SerializeField] private TextAsset inkJSON;
    void Start()
    {
        Debug.Log("Dialogue started");
        Debug.Log(inkJSON.text);
        DialogueManager.instance.StartDialogue(inkJSON);
    }
}
