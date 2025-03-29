using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;
    private TMP_Text[] choicesText;

    private Story currentStory;
    private bool isPlaying = false;

    public bool IsPlaying { get => isPlaying; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }else if (instance != this)
        {
            Destroy(gameObject);
        }

        // Find the UI elements for the dialogue
        dialoguePanel =         GameObject.Find("Canvas/DialoguePanel");
        nameText =              GameObject.Find("Canvas/DialoguePanel/NameTxt").GetComponent<TMP_Text>();
        dialogueText =          GameObject.Find("Canvas/DialoguePanel/DialogueTxt").GetComponent<TMP_Text>();

        // Find the UI elements for the choices
        GameObject choicesPanel = GameObject.Find("Canvas/DialogueChoices");
        choices = new GameObject[choicesPanel.transform.childCount];
        for (int i = 0; i < choicesPanel.transform.childCount; i++)
        {
            choices[i] = choicesPanel.transform.GetChild(i).gameObject;
            Debug.Log(choices[i].name);
        }

        choicesText = new TMP_Text[choices.Length];
        Debug.Log(choices.Length);
        for (int i = 0; i < choices.Length; i++)
        {
            choicesText[i] = choices[i].GetComponentInChildren<TMP_Text>();
        }
    }

    private void Start()
    {
        isPlaying = false;
        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            ContinueDialogue();
        }
    }
    public void StartDialogue(TextAsset inkJSON)
    {
        Debug.Log("Starting dialogue");
        currentStory = new Story(inkJSON.text);
        isPlaying = true;
        dialoguePanel.SetActive(true);
        ContinueDialogue();
    }

    public void ContinueDialogue()
    {
        if (currentStory.canContinue)
        {
            string text = currentStory.Continue();
            nameText.text = text;
            dialogueText.text = text;

            DisplayChoices();
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        isPlaying = false;
        dialoguePanel.SetActive(false);
    }

    public void DisplayChoices()
    {
        // Hide all choices
        for (int i = 0; i < choices.Length; i++)
        {
            choices[i].SetActive(false);
        }

        // Find the current choices
        List<Choice> currentChoices = new List<Choice>();

        // A catch in case the currentStory choices exceed the number of buttons
        //if (currentStory.currentChoices.Count > choices.Length)
        //{
        //    Debug.LogError($"There are more choices than buttons {choices.Length}");
        //    return;
        //}

        // Display the available choices from the current story
        for (int i = 0; i < currentStory.currentChoices.Count; i++)
        {
            Debug.Log(currentStory.currentChoices[i].text);
            if (i >= choices.Length)
            {
                Debug.LogError($"There are more choices than buttons {choices.Length}");
                return;
            }
            choices[i].SetActive(true);
            Debug.Log(choicesText[i]);
            Debug.Log(currentStory.currentChoices[i].text);
            choicesText[i].text = currentStory.currentChoices[i].text;
        }
    }
}
