using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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

    private const string SPEAKER = "speaker";
    private const string PORTRAIT = "portrait";
    private const string LAYOUT = "layout";

    private Story currentStory;
    private bool isPlaying = false;

    public bool IsPlaying { get => isPlaying; }     // Return the current state of the dialogue

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        // Find the UI elements for the dialogue
        dialoguePanel = GameObject.Find("Canvas/DialoguePanel");
        nameText = GameObject.Find("Canvas/DialoguePanel/NameTxt").GetComponent<TMP_Text>();
        dialogueText = GameObject.Find("Canvas/DialoguePanel/DialogueTxt").GetComponent<TMP_Text>();
        GameObject choicesPanel = GameObject.Find("Canvas/ChoicesPanel");

        if (dialoguePanel is null) Debug.LogError("Dialogue Panel not found");
        if (nameText is null) Debug.LogError("Name Text not found");
        if (dialogueText is null) Debug.LogError("Dialogue Text not found");
        if (choicesPanel is null) Debug.LogError("Choices Panel not found");

        // Find the UI elements for the choices
        choices = new GameObject[choicesPanel.transform.childCount];
        for (int i = 0; i < choicesPanel.transform.childCount; i++)
        {
            choices[i] = choicesPanel.transform.GetChild(i).gameObject;
            int index = i;

            // Add a listener to the button to handle the choice selection
            choices[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("Choice " + index);
                currentStory.ChooseChoiceIndex(index);
                ContinueDialogue();
            });
        }

        choicesText = new TMP_Text[choices.Length];
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

        //if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        //{
        //    ContinueDialogue();
        //}
    }
    public void StartDialogue(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        isPlaying = true;
        dialoguePanel.SetActive(true);
        ContinueDialogue();
    }

    public void ContinueDialogue()
    {
        if (currentStory.canContinue)
        {
            //Display the dialogue text
            string text = currentStory.Continue();
            dialogueText.text = text;

            // Display the player choices
            DisplayChoices();

            // Handle the tags
            HandleTags(currentStory.currentTags);
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

    /// <summary>
    /// Handle the tags written in INK
    /// </summary>
    /// <param name="tags"> A list of strings that are stored in INK called tags</param>
    private void HandleTags(List<string> tags)
    {
        foreach (string tag in tags)
        {
            string[] tagData = tag.Split(':');
            if (tagData.Length > 2)
            {
                Debug.LogWarning($"Tag cannot be parsed: {tag}");
                continue;
            }
            string tagName = tagData[0].Trim();     //Example: speaker
            string tagValue = tagData[1].Trim();    //Example: Aelius

            switch (tagName)
            {
                case SPEAKER:
                    nameText.text = tagValue;
                    break;
                case PORTRAIT:
                    Debug.Log($"Portrait {tagValue}");
                    break;
                case LAYOUT:
                    Debug.Log($"Layout {tagValue}");
                    break;
                default:
                    Debug.LogWarning("Unknown tag: " + tagName);
                    break;
            }
        }
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
            if (i >= choices.Length)
            {
                Debug.LogError($"There are more choices than buttons {choices.Length}");
                return;
            }
            choices[i].SetActive(true);
            choicesText[i].text = currentStory.currentChoices[i].text;
        }

        StartCoroutine(SelectFirstChoice());
    }

    private IEnumerator SelectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0]);
    }

    private IEnumerator TypeWriter()
    {
        yield return null;
    }
}
