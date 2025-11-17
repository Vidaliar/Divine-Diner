using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDialogueEnter : MonoBehaviour
{
    [SerializeField] private TextAsset inkJSON;
    void Start()
    {
        DialogueManager.instance.StartDialogue(inkJSON);
    }
}
