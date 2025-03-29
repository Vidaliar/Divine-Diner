using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance;
    [SerializeField] private List<Character> characters = new List<Character>();
    public Dictionary<string, Character> Characters = new Dictionary<string, Character>();

    private string currentSpeaker;
    public string CurrentSpeaker { get => currentSpeaker; set => currentSpeaker = value; }

    private void Awake()
    {
        if (instance == null && instance != this)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        ResetCharacter();

        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Character");
        foreach (GameObject go in gameObjects)
        {
            Character character = go.GetComponent<Character>();
            characters.Add(character);
        }

        for (int i = 0; i < characters.Count; i++)
        {
            Characters.Add(characters[i].Name, characters[i]);
            Debug.Log("Character: " + characters[i].Name + " added to the dictionary");
        }

        DontDestroyOnLoad(gameObject);
    }

    public static void ResetCharacter()
    {
        instance.characters.Clear();
    }

    public void SetCharacterMood(string characterName, Enum_Mood mood)
    {
        if (Characters.ContainsKey(characterName))
        {
            Characters[characterName].SetMood(mood);
        }
        else
        {
            Debug.LogError("Character not found");
        }
    }

    public void SetSpeackerMood(Enum_Mood mood)
    {
        Characters[currentSpeaker].SetMood(mood);
    }
}
