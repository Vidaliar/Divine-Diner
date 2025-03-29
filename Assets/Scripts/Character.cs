using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Character : MonoBehaviour
{
    [SerializeField] private CharacterSO characterSO;
    [SerializeField] private Image portrait;
    public string Name => characterSO.characterName;

    private void Awake()
    {
        portrait = gameObject.GetComponent<Image>();
        SetMood(Enum_Mood.Neutral);
    }

    public void SetMood(Enum_Mood mood)
    {
        portrait.sprite = characterSO.GetSprite(mood);
    }
}
