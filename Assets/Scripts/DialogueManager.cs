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

        dialoguePanel =         GameObject.Find("Canvas/Dialogue");
        nameText =              GameObject.Find("Canvas/Dialogue/NameTxt").GetComponent<TMP_Text>();
        dialogueText =          GameObject.Find("Canvas/Dialogue/DialogueTxt").GetComponent<TMP_Text>();
    }

    private void Start()
    {
        isPlaying = false;
        dialoguePanel.SetActive(false);

        choicesText = new TMP_Text[choices.Length];
        for (int i = 0; i < choices.Length; i++)
        {
            choicesText[i] = choices[i].GetComponent<TMP_Text>();
        }
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            DisplayNextLine();
        }
    }
    public void StartDialogue(TextAsset inkJSON)
    {
        Debug.Log("Starting dialogue");
        currentStory = new Story(inkJSON.text);
        isPlaying = true;
        dialoguePanel.SetActive(true);
        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if (currentStory.canContinue)
        {
            string text = currentStory.Continue();
            nameText.text = text;
            dialogueText.text = text;
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
        List<Choice> currentChoices = new List<Choice>();

        if (currentStory.currentChoices.Count > choices.Length)
        {
            Debug.LogError($"There are more choices than buttons {choices.Length}");
            return;
        }

        for (int i = 0; i < currentStory.currentChoices.Count; i++)
        {
            currentChoices.Add(currentStory.currentChoices[i]);
        }
    }
}
