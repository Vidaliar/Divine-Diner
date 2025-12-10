using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Character")]
public class CharacterSO : ScriptableObject
{
    [Header("Character Information")]
    [SerializeField] public string characterName;
    [SerializeField] private Sprite Neutral;
    [SerializeField] private Sprite Happy;
    [SerializeField] private Sprite Angry;
    [SerializeField] private Sprite Sad;
    public Sprite GetSprite(Enum_Mood mood)
    {
        Debug.Log("Getting sprite for " + characterName + " with mood " + mood);
        switch (mood)
        {
            case Enum_Mood.Neutral:
                return Neutral;
            case Enum_Mood.Happy:
                Debug.Log("Happy");
                return Happy;
            case Enum_Mood.Sad:
                return Sad;
            case Enum_Mood.Angry:
                return Angry;
            default:
                return Neutral;
        }
    }
}
